// Theme interop: toggle dark class on <html> element
window.themeInterop = {
    setDarkMode: function (isDark) {
        if (isDark) {
            document.documentElement.classList.add('dark');
        } else {
            document.documentElement.classList.remove('dark');
        }
        try {
            localStorage.setItem('dashboard-theme', isDark ? 'dark' : 'light');
        } catch (e) { }
    },
    getSavedTheme: function () {
        try {
            return localStorage.getItem('dashboard-theme');
        } catch (e) {
            return null;
        }
    }
};

// File read helper — opens a file picker and returns file content as text
window.readFileAsText = function () {
    return new Promise((resolve, reject) => {
        const input = document.createElement('input');
        input.type = 'file';
        input.accept = '.json,application/json';
        input.onchange = (e) => {
            const file = e.target.files[0];
            if (!file) { reject('No file selected'); return; }
            const reader = new FileReader();
            reader.onload = (ev) => resolve(ev.target.result);
            reader.onerror = () => reject('Failed to read file');
            reader.readAsText(file);
        };
        input.click();
    });
};

// File download helper
window.downloadFile = function (filename, content) {
    const blob = new Blob([content], { type: 'application/json;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
};
