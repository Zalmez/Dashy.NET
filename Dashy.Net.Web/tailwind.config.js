/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Components/**/*.{razor,html,cshtml}",
    "./Pages/**/*.{razor,html,cshtml}",
    "./Shared/**/*.{razor,html,cshtml}",
    "./wwwroot/**/*.html"
  ],
  darkMode: 'class', // Enable class-based dark mode
  theme: {
    extend: {
      colors: {
        // Design tokens - can be overridden by CSS variables
        primary: 'var(--accent-color)',
        'primary-dark': 'var(--accent-color-darker)',
        danger: 'var(--danger-color)',
        'danger-hover': 'var(--danger-hover-color)',
      },
      spacing: {
        // Additional spacing if needed
      },
      borderRadius: {
        // Can add custom radius values
      },
    },
  },
  plugins: [],
}
