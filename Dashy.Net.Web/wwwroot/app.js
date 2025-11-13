function setBodyClass(cssClass) {
  document.body.className = cssClass;
}

function saveToLocalStorage(key, value) {
  localStorage.setItem(key, value);
}

function getFromLocalStorage(key) {
  return localStorage.getItem(key);
}

function addClickOutsideHandler(elementId, dotNetRef) {
  document.addEventListener("click", function (event) {
    const element = document.getElementById(elementId);
    if (element && !element.contains(event.target)) {
      dotNetRef.invokeMethodAsync("CloseDropdown");
    }
  });
}

function setCustomBackground(imagePath) {
  console.log('setCustomBackground called with:', imagePath);
  const dashboardElement = document.querySelector('.dashboard-layout');
  console.log('Dashboard layout element found:', dashboardElement);
  if (dashboardElement && imagePath) {
    dashboardElement.style.setProperty('--custom-background-image', `url(${imagePath})`);
    dashboardElement.classList.add('has-custom-background');
    console.log('Background image applied successfully');
  } else {
    console.log('Failed to apply background - missing dashboard element or image path');
  }
}

function removeCustomBackground() {
  console.log('removeCustomBackground called');
  const dashboardElement = document.querySelector('.dashboard-layout');
  if (dashboardElement) {
    dashboardElement.style.removeProperty('--custom-background-image');
    dashboardElement.classList.remove('has-custom-background');
    console.log('Background image removed successfully');
  }
}

// Dynamically add/replace a theme stylesheet. Useful for theme packs.
function setThemePack(href) {
  if (!href) return;
  let link = document.getElementById('dashy-theme-pack');
  if (!link) {
    link = document.createElement('link');
    link.rel = 'stylesheet';
    link.id = 'dashy-theme-pack';
    document.head.appendChild(link);
  }
  link.href = href;
}

function getViewportWidth() {
  return window.innerWidth || document.documentElement.clientWidth || 0;
}

// Test function to verify CSS is working
function testBackground() {
  console.log('testBackground called');
  const dashboardElement = document.querySelector('.dashboard-layout');
  if (dashboardElement) {
    // Use a simple gradient as a test
    dashboardElement.style.setProperty('--custom-background-image', 'linear-gradient(45deg, #ff6b6b, #4ecdc4)');
    dashboardElement.classList.add('has-custom-background');
    console.log('Test background applied');
  }
}
