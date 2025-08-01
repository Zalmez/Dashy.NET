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
  const pageElement = document.querySelector('.page');
  if (pageElement && imagePath) {
    pageElement.style.setProperty('--custom-background-image', `url(${imagePath})`);
    pageElement.classList.add('has-custom-background');
  }
}

function removeCustomBackground() {
  const pageElement = document.querySelector('.page');
  if (pageElement) {
    pageElement.style.removeProperty('--custom-background-image');
    pageElement.classList.remove('has-custom-background');
  }
}
