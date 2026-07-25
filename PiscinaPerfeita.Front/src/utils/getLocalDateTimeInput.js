/**
 * Retorna a data/hora local atual formatada para inputs do tipo datetime-local (YYYY-MM-THH:mm).
 */
export const getLocalDateTimeInput = (date = new Date()) => {
  const localDate = new Date(date);
  localDate.setMinutes(localDate.getMinutes() - localDate.getTimezoneOffset());
  return localDate.toISOString().slice(0, 16);
};
