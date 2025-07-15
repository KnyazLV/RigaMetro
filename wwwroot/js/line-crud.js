/**
 * Module for CRUD operations with metro lines in the admin panel.
 * Requires transfer of localization object.
 */

export function initLineCrud(localization) {
  //#region Helpers

  function timeToMinutes(timeStr) {
    const arr = timeStr.split(':');
    if (arr.length !== 2) return 0;
    const [hours, minutes] = arr.map(Number);
    return hours * 60 + minutes;
  }

  function getRowInputs(row) {
    return {
      start: row.querySelector('input[name="StartWorkTime"]'),
      end: row.querySelector('input[name="EndWorkTime"]'),
      color: row.querySelector('input[name="Color"]')
    };
  }

  function getCsrfToken() {
    return document.querySelector('input[name="__RequestVerificationToken"]')?.value;
  }

  //#endregion

  //#region Edit/Save Logic

  document.querySelectorAll('.edit-line-btn').forEach(btn => {
    btn.addEventListener('click', () => {
      const row = btn.closest('tr');
      const inputs = getRowInputs(row);
      const isSaving = btn.textContent.trim() === localization.save;

      if (!isSaving) {
        inputs.start.disabled = false;
        inputs.end.disabled = false;
        inputs.color.disabled = false;

        btn.textContent = localization.save;
        btn.classList.replace('btn-outline-primary', 'btn-primary');
      } else {
        if (timeToMinutes(inputs.start.value) >= timeToMinutes(inputs.end.value)) {
          alert(localization.alertStartTime);
          return;
        }

        const data = {
          LineID: row.dataset.lineId,
          StartWorkTime: inputs.start.value,
          EndWorkTime: inputs.end.value,
          Color: inputs.color.value
        };

        fetch('/Admin/UpdateLine', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': getCsrfToken()
          },
          body: JSON.stringify(data)
        })
          .then(r => {
            if (!r.ok) return r.text().then(t => { throw new Error(t); });
            location.reload();
          })
          .catch(err => {
            alert(err.message || localization.failedToUpdateLine);
          });
      }
    });
  });

  //#endregion
}
