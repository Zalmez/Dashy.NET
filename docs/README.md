# Dashy.NET Documentation

This directory contains the documentation for Dashy.NET built with [Docusaurus](https://docusaurus.io/).

## Development

### Prerequisites

- Node.js (version 18 or above)
- npm or yarn

### Installation

```bash
cd docs
npm install
```

### Local Development

```bash
npm start
```

This command starts a local development server and opens up a browser window. Most changes are reflected live without having to restart the server.

### Build

```bash
npm run build
```

This command generates static content into the `build` directory and can be served using any static contents hosting service.

## Deployment

The documentation is automatically deployed to GitHub Pages when changes are pushed to the main branch. The deployment is handled by the GitHub Actions workflow in `.github/workflows/docs.yml`.

### Manual Deployment

```bash
npm run deploy
```

If you are using GitHub pages for hosting, this command is a convenient way to build the website and push to the `gh-pages` branch.

## Structure

```
docs/
├── docs/                    # Documentation pages
│   ├── intro.md            # Introduction page
│   ├── installation.md     # Installation guide
│   ├── configuration.md    # Configuration guide
│   ├── docker.md           # Docker deployment guide
│   └── tutorial-basics/    # Tutorial pages
├── blog/                   # Blog posts (optional)
├── src/                    # React components
│   ├── components/         # Custom components
│   └── pages/             # Custom pages
├── static/                 # Static assets
│   └── img/               # Images including dashynet-logo.png
├── .env.example           # Environment configuration template
├── docusaurus.config.ts   # Configuration file
└── sidebars.ts            # Sidebar configuration
```

## Contributing

When contributing to the documentation:

1. Make changes to the appropriate files in the `docs/` directory
2. Test locally with `npm start`
3. Submit a pull request

The documentation will be automatically built and deployed when merged to main.
