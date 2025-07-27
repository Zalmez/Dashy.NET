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
