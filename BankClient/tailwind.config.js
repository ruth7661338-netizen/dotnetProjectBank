/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  theme: {
    extend: {
      colors: {
        ink: '#0f1b33',        // טקסט ראשי - כחול-נייבי כהה
        brand: {
          DEFAULT: '#1d4ed8',  // צבע המותג - כפתורים, קישורים, הדגשות
          dark: '#1e3a8a',     // hover וגבולות עדינים
          soft: '#dbeafe'      // רקע לתגיות
        },
        canvas: '#f3f6fb'      // רקע הדף
      },
      fontFamily: {
        sans: ['Arial', 'sans-serif']
      }
    }
  },
  plugins: []
};
