function setupRigaClock(elemId, withDate = false) {
  const elem = document.getElementById(elemId);
  if (!elem) return;
  function update() {
    const now = new Date();
    const timeOptions = { hour: '2-digit', minute: '2-digit', hour12: false, timeZone: 'Europe/Riga' };
    const timeParts = new Intl.DateTimeFormat('en-GB', timeOptions).formatToParts(now);
    const hour = timeParts.find(p => p.type === 'hour').value;
    const minute = timeParts.find(p => p.type === 'minute').value;
    const colon = now.getSeconds() % 2 === 0 ? '<span class="blink">:</span>' : ':';

    let html = `<p class="riga-time-clock">${hour}${colon}${minute}</p>`;
    if (withDate) {
      const dateOptions = { weekday: 'long', month: 'long', year: 'numeric', day: 'numeric', timeZone: 'Europe/Riga' };
      const date = new Intl.DateTimeFormat('en-US', dateOptions).format(now);
      const dateParts = date.split(' ');
      const dayName = dateParts[0].replace(',', '');
      const monthName = dateParts[1].replace(',', '');
      const year = dateParts[3];
      const dateStr = `${dayName}, ${monthName} ${year}`;
      html += `<p class="riga-time-date">${dateStr}</p>`;
    }
    elem.innerHTML = html;
  }
  update();
  setInterval(update, 1000);
}
