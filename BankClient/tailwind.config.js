/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  theme: {
    extend: {
      colors: {
        ink: '#172b2a',
        forest: '#145b4a',
        mint: '#dff3e9',
        canvas: '#f4f7f3'
      },
      fontFamily: {
        sans: ['Arial', 'sans-serif']
      }
    }
  },
  plugins: []
};