// אותה בדיקה כמו IsraeliIdAttribute בשרת: 9 ספרות + ספרת ביקורת.
// השרת הוא שאוכף בפועל - כאן זה רק כדי להציג שגיאה מיד בלי לשלוח בקשה.
export function isValidIsraeliId(id) {
  if (!/^\d{9}$/.test(id)) return false;

  let sum = 0;
  for (let i = 0; i < 9; i++) {
    const product = Number(id[i]) * (i % 2 === 0 ? 1 : 2);
    sum += product > 9 ? product - 9 : product;
  }
  return sum % 10 === 0;
}
