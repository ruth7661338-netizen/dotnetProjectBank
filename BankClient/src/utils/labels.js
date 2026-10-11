// השרת יכול להחזיר enum כשם ("Deposit") או כמספר (0) - שתי הצורות ממופות לתווית בעברית

const ACCOUNT_TYPE_LABELS = {
  0: 'עובר ושב',
  1: 'חיסכון',
  Checking: 'עובר ושב',
  Savings: 'חיסכון'
};

const TRANSACTION_TYPE_LABELS = {
  0: 'הפקדה',
  1: 'משיכה',
  2: 'העברה יוצאת',
  3: 'העברה נכנסת',
  Deposit: 'הפקדה',
  Withdraw: 'משיכה',
  TransferOut: 'העברה יוצאת',
  TransferIn: 'העברה נכנסת'
};

export function getAccountTypeLabel(type) {
  return ACCOUNT_TYPE_LABELS[type] ?? type;
}

export function getTransactionTypeLabel(type) {
  return TRANSACTION_TYPE_LABELS[type] ?? type;
}
