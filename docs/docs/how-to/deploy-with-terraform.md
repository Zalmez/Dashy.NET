---
sidebar_position: 3
---

# How-to: Deploy Dashy to Azure with Terraform

## Use This Guide When

You want to provision the full Dashy stack on Azure using Terraform and deploy it as a
set of Azure Container Apps. Use this guide if you need a repeatable, version-controlled
infrastructure deployment rather than a one-off manual setup.

This guide does **not** cover local development (see
[Local Development](../getting-started/local-development.md)) or CI/CD pipeline
configuration.

---

## Prerequisites

Before you begin, ensure the following are installed and configured:

| Tool | Version | Purpose |
|---|---|---|
| [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) | Latest | Authenticate to Azure |
| [Terraform CLI](https://developer.hashicorp.com/terraform/install) | ≥ 1.6 | Provision infrastructure |
| Docker | Latest | Build and push container images |

You also need:

- An active Azure subscription.
- Contributor (or Owner) access on the target subscription or resource group.
- Container images for `dashy3.ApiService` and `dashy3.Web` built and ready to push.

---

## Infrastructure Overview

The Terraform configuration provisions the following Azure resources:

| Resource | Purpose |
|---|---|
| Resource Group | Logical container for all Dashy resources |
| Azure Container Registry (ACR) | Private registry for Dashy container images |
| PostgreSQL Flexible Server | Primary database |
| Azure Cache for Redis | Session and application cache |
| Container Apps Environment | Shared hosting environment for all containers |
| Container App — `dashy-migration` | Runs database migrations on startup |
| Container App — `dashy-api` | Backend API service |
| Container App — `dashy-web` | Blazor web frontend |

---

## Terraform File Structure

Create a dedicated directory for the Terraform configuration, for example `infra/azure/`:

```
infra/azure/
├── main.tf
├── variables.tf
└── outputs.tf
```

### `variables.tf`

```hcl
variable "resource_group_name" {
  type        = string
  description = "Name of the Azure Resource Group."
}

variable "location" {
  type        = string
  description = "Azure region to deploy into (e.g. northeurope)."
  default     = "northeurope"
}

variable "acr_name" {
  type        = string
  description = "Globally unique name for the Azure Container Registry."
}

variable "db_admin_password" {
  type        = string
  sensitive   = true
  description = "Administrator password for the PostgreSQL server."
}

variable "api_image" {
  type        = string
  description = "Full image reference for the API service (e.g. myacr.azurecr.io/dashy-api:latest)."
}

variable "web_image" {
  type        = string
  description = "Full image reference for the web frontend (e.g. myacr.azurecr.io/dashy-web:latest)."
}

variable "migration_image" {
  type        = string
  description = "Full image reference for the migration service (e.g. myacr.azurecr.io/dashy-migration:latest)."
}
```

### `main.tf`

```hcl
terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.100"
    }
  }
}

provider "azurerm" {
  features {}
}

# ── Resource Group ────────────────────────────────────────────────────────────

resource "azurerm_resource_group" "dashy" {
  name     = var.resource_group_name
  location = var.location
}

# ── Container Registry ────────────────────────────────────────────────────────

resource "azurerm_container_registry" "dashy" {
  name                = var.acr_name
  resource_group_name = azurerm_resource_group.dashy.name
  location            = azurerm_resource_group.dashy.location
  sku                 = "Basic"
  admin_enabled       = true
}

# ── PostgreSQL Flexible Server ────────────────────────────────────────────────

resource "azurerm_postgresql_flexible_server" "dashy" {
  name                   = "dashy-postgres"
  resource_group_name    = azurerm_resource_group.dashy.name
  location               = azurerm_resource_group.dashy.location
  version                = "16"
  administrator_login    = "dashy_admin"
  administrator_password = var.db_admin_password
  storage_mb             = 32768
  sku_name               = "B_Standard_B1ms"

  authentication {
    active_directory_auth_enabled = false
    password_auth_enabled         = true
  }
}

resource "azurerm_postgresql_flexible_server_database" "dashy" {
  name      = "dashy"
  server_id = azurerm_postgresql_flexible_server.dashy.id
  collation = "en_US.utf8"
  charset   = "utf8"
}

resource "azurerm_postgresql_flexible_server_firewall_rule" "allow_azure" {
  name             = "AllowAzureServices"
  server_id        = azurerm_postgresql_flexible_server.dashy.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

# ── Redis Cache ───────────────────────────────────────────────────────────────

resource "azurerm_redis_cache" "dashy" {
  name                = "dashy-redis"
  resource_group_name = azurerm_resource_group.dashy.name
  location            = azurerm_resource_group.dashy.location
  capacity            = 0
  family              = "C"
  sku_name            = "Basic"
  minimum_tls_version = "1.2"
}

# ── Container Apps Environment ────────────────────────────────────────────────

resource "azurerm_container_app_environment" "dashy" {
  name                = "dashy-env"
  resource_group_name = azurerm_resource_group.dashy.name
  location            = azurerm_resource_group.dashy.location
}

# ── Shared locals ─────────────────────────────────────────────────────────────

locals {
  db_connection_string = join(";", [
    "Host=${azurerm_postgresql_flexible_server.dashy.fqdn}",
    "Port=5432",
    "Database=dashy",
    "Username=dashy_admin",
    "Password=${var.db_admin_password}",
    "SslMode=Require"
  ])

  redis_connection_string = "${azurerm_redis_cache.dashy.hostname}:${azurerm_redis_cache.dashy.ssl_port},password=${azurerm_redis_cache.dashy.primary_access_key},ssl=True,abortConnect=False"

  registry_credentials = [{
    server               = azurerm_container_registry.dashy.login_server
    username             = azurerm_container_registry.dashy.admin_username
    password_secret_name = "acr-password"
  }]

  acr_secret = [{
    name  = "acr-password"
    value = azurerm_container_registry.dashy.admin_password
  }]
}

# ── Migration Container App ───────────────────────────────────────────────────

resource "azurerm_container_app" "migration" {
  name                         = "dashy-migration"
  resource_group_name          = azurerm_resource_group.dashy.name
  container_app_environment_id = azurerm_container_app_environment.dashy.id
  revision_mode                = "Single"

  secret {
    name  = "acr-password"
    value = azurerm_container_registry.dashy.admin_password
  }

  secret {
    name  = "db-connection-string"
    value = local.db_connection_string
  }

  registry {
    server               = azurerm_container_registry.dashy.login_server
    username             = azurerm_container_registry.dashy.admin_username
    password_secret_name = "acr-password"
  }

  template {
    min_replicas = 0
    max_replicas = 1

    container {
      name   = "migration"
      image  = var.migration_image
      cpu    = 0.25
      memory = "0.5Gi"

      env {
        name        = "ConnectionStrings__DefaultConnection"
        secret_name = "db-connection-string"
      }
    }
  }
}

# ── API Container App ─────────────────────────────────────────────────────────

resource "azurerm_container_app" "api" {
  name                         = "dashy-api"
  resource_group_name          = azurerm_resource_group.dashy.name
  container_app_environment_id = azurerm_container_app_environment.dashy.id
  revision_mode                = "Single"

  secret {
    name  = "acr-password"
    value = azurerm_container_registry.dashy.admin_password
  }

  secret {
    name  = "db-connection-string"
    value = local.db_connection_string
  }

  secret {
    name  = "redis-connection-string"
    value = local.redis_connection_string
  }

  registry {
    server               = azurerm_container_registry.dashy.login_server
    username             = azurerm_container_registry.dashy.admin_username
    password_secret_name = "acr-password"
  }

  ingress {
    external_enabled = false
    target_port      = 8080

    traffic_weight {
      percentage      = 100
      latest_revision = true
    }
  }

  template {
    min_replicas = 1
    max_replicas = 3

    container {
      name   = "api"
      image  = var.api_image
      cpu    = 0.5
      memory = "1Gi"

      env {
        name        = "ConnectionStrings__DefaultConnection"
        secret_name = "db-connection-string"
      }

      env {
        name        = "ConnectionStrings__Redis"
        secret_name = "redis-connection-string"
      }

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = "Production"
      }
    }
  }

  depends_on = [azurerm_container_app.migration]
}

# ── Web Frontend Container App ────────────────────────────────────────────────

resource "azurerm_container_app" "web" {
  name                         = "dashy-web"
  resource_group_name          = azurerm_resource_group.dashy.name
  container_app_environment_id = azurerm_container_app_environment.dashy.id
  revision_mode                = "Single"

  secret {
    name  = "acr-password"
    value = azurerm_container_registry.dashy.admin_password
  }

  registry {
    server               = azurerm_container_registry.dashy.login_server
    username             = azurerm_container_registry.dashy.admin_username
    password_secret_name = "acr-password"
  }

  ingress {
    external_enabled = true
    target_port      = 8080

    traffic_weight {
      percentage      = 100
      latest_revision = true
    }
  }

  template {
    min_replicas = 1
    max_replicas = 3

    container {
      name   = "web"
      image  = var.web_image
      cpu    = 0.5
      memory = "1Gi"

      env {
        name  = "ApiService__BaseUrl"
        value = "https://${azurerm_container_app.api.ingress[0].fqdn}"
      }

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = "Production"
      }
    }
  }

  depends_on = [azurerm_container_app.api]
}
```

### `outputs.tf`

```hcl
output "web_url" {
  description = "Public URL of the Dashy web frontend."
  value       = "https://${azurerm_container_app.web.ingress[0].fqdn}"
}

output "acr_login_server" {
  description = "Login server for the Azure Container Registry."
  value       = azurerm_container_registry.dashy.login_server
}

output "postgres_fqdn" {
  description = "Fully qualified domain name of the PostgreSQL server."
  value       = azurerm_postgresql_flexible_server.dashy.fqdn
}
```

---

## Steps

### 1. Authenticate to Azure

```bash
az login
az account set --subscription "<your-subscription-id>"
```

### 2. Initialise Terraform

From the `infra/azure/` directory:

```bash
terraform init
```

### 3. Create a `terraform.tfvars` file

Create `infra/azure/terraform.tfvars` with your specific values. **Do not commit this
file to source control** — add it to `.gitignore`.

```hcl
resource_group_name = "dashy-prod-rg"
location            = "northeurope"
acr_name            = "dashyacr"            # must be globally unique
db_admin_password   = "a-strong-password"   # use a secrets manager in production

# Set these after pushing your images in step 5
api_image           = "dashyacr.azurecr.io/dashy-api:latest"
web_image           = "dashyacr.azurecr.io/dashy-web:latest"
migration_image     = "dashyacr.azurecr.io/dashy-migration:latest"
```

### 4. Plan and review

```bash
terraform plan
```

Review the output to confirm the expected resources will be created before proceeding.

### 5. Apply the infrastructure

```bash
terraform apply
```

Confirm when prompted. Terraform will provision the resource group, ACR, PostgreSQL,
Redis, and Container Apps Environment. The Container Apps themselves will initially
fail their health checks because images have not been pushed yet — this is expected.

### 6. Push container images to ACR

Log in to the registry using the login server output from Terraform:

```bash
ACR=$(terraform output -raw acr_login_server)

az acr login --name "$ACR"

# Build and push each image
docker build -f dashy3.ApiService/Dockerfile -t "$ACR/dashy-api:latest" .
docker push "$ACR/dashy-api:latest"

docker build -f dashy3.MigrationService/Dockerfile -t "$ACR/dashy-migration:latest" .
docker push "$ACR/dashy-migration:latest"

docker build -f dashy3.Web/Dockerfile -t "$ACR/dashy-web:latest" .
docker push "$ACR/dashy-web:latest"
```

### 7. Trigger a new revision

After pushing images, force the Container Apps to pull the latest images:

```bash
az containerapp update --name dashy-migration \
  --resource-group dashy-prod-rg \
  --image "$ACR/dashy-migration:latest"

az containerapp update --name dashy-api \
  --resource-group dashy-prod-rg \
  --image "$ACR/dashy-api:latest"

az containerapp update --name dashy-web \
  --resource-group dashy-prod-rg \
  --image "$ACR/dashy-web:latest"
```

---

## Verification

Once all Container Apps report a healthy revision:

1. Retrieve the public URL:

   ```bash
   terraform output web_url
   ```

2. Open the URL in a browser. The Dashy login or dashboard page should load.

3. Confirm the API is reachable by checking Container App logs:

   ```bash
   az containerapp logs show --name dashy-api \
     --resource-group dashy-prod-rg \
     --follow
   ```

4. Confirm database connectivity by verifying the migration service exited successfully:

   ```bash
   az containerapp logs show --name dashy-migration \
     --resource-group dashy-prod-rg
   ```

---

## Teardown

To remove all provisioned resources:

```bash
terraform destroy
```

This is irreversible. Confirm you have backed up any data before running this command.

---

## Troubleshooting

### Container App fails to start with `ImagePullBackOff`

- Confirm the image was pushed to ACR before the Container App started.
- Verify the image reference in `terraform.tfvars` matches the tag you pushed.
- Check that ACR admin credentials are correct:
  ```bash
  az acr credential show --name dashyacr
  ```

### API reports database connection errors

- Confirm the PostgreSQL firewall rule allows Azure services (`0.0.0.0` to `0.0.0.0`).
- Verify the connection string environment variable is set correctly on the Container App.
- Check that the migration service completed successfully before the API started.

### Web frontend loads but API calls fail

- Confirm `ApiService__BaseUrl` on the web Container App points to the internal FQDN of
  `dashy-api`.
- Ensure the API ingress is set to `external_enabled = false` and that the web and API
  apps are in the same Container Apps Environment.

### Authentication is broken after deployment

- Follow [How-to: Configure OIDC](./configure-oidc.md) to configure your identity
  provider with the new public URL of the web frontend.
- Update the callback/redirect URI in your identity provider to match the `web_url`
  output from Terraform.
