/**
 * Module for CRUD operations with trains on the metro admin page.
 * Requires transfer of localization objects (edit/save/remove texts and notifications).
 * All actions are performed via fetch and use RequestVerificationToken for security.
 */

export function initTrainCrud(localization) {

  //#region Helpers
  function timeToMinutes(timeStr) {
    const [hours, minutes] = timeStr.split(':').map(Number);
    return hours * 60 + minutes;
  }

  // Get all elements of row
  function getRowInputs(row) {
    return {
      name: row.querySelector('input[name="TrainName"]'),
      line: row.querySelector('select[name="LineID"]'),
      start: row.querySelector('input[name="StartWorkTime"]'),
      end: row.querySelector('input[name="EndWorkTime"]'),
      statusDisp: row.querySelector('.status-display'),
      statusSel: row.querySelector('.status-editor')
    };
  }

  function getCsrfToken() {
    return document.querySelector('input[name="__RequestVerificationToken"]')?.value;
  }

  // Get data about the train
  function getTrainData(row, inputs) {
    return {
      TrainID: row.dataset.trainId,
      TrainName: inputs.name.value,
      LineID: inputs.line.value,
      StartWorkTime: inputs.start.value,
      EndWorkTime: inputs.end.value,
      IsActive: inputs.statusSel.value === 'true'
    };
  }

  //#endregion

  //#region Train Edit

  document.querySelectorAll('.edit-btn').forEach(btn => {
    btn.addEventListener('click', () => {
      const row = btn.closest('tr');
      const inputs = getRowInputs(row);
      const isSaving = btn.textContent.trim() === localization.save;

      if (!isSaving) {
        inputs.name.disabled = false;
        inputs.line.disabled = false;
        inputs.start.disabled = false;
        inputs.end.disabled = false;
        inputs.statusDisp.classList.add('d-none');
        inputs.statusSel.classList.remove('d-none');
        inputs.statusSel.value = inputs.statusSel.dataset.value;

        btn.textContent = localization.save;
        btn.classList.replace('btn-outline-primary', 'btn-primary');
      } else {
        if (timeToMinutes(inputs.start.value) >= timeToMinutes(inputs.end.value)) {
          alert(localization.alertStartTime);
          return;
        }

        const formData = getTrainData(row, inputs);

        fetch('/Admin/UpdateTrain', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': getCsrfToken()
          },
          body: JSON.stringify(formData)
        })
          .then(r => {
            if (!r.ok) throw new Error(localization.failedToSave);
            location.reload();
          })
          .catch(() => {
            alert(localization.failedToSave);
          });
      }
    });
  });

  //#endregion

  //#region Train Delete

  document.querySelectorAll('.delete-btn').forEach(btn => {
    btn.addEventListener('click', () => {
      const id = btn.closest('tr').dataset.trainId;
      if (!confirm(localization.removeTrain)) return;

      fetch('/Admin/DeleteTrain', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'RequestVerificationToken': getCsrfToken()
        },
        body: JSON.stringify(id)
      })
        .then(r => {
          if (!r.ok) return r.text().then(t => { throw new Error(t); });
          location.reload();
        })
        .catch(err => alert(err.message || localization.failedToDelete));
    });
  });

  //#endregion

  //#region New Train Creation

  const createBtn = document.getElementById('createTrainBtn');
  if (createBtn) {
    createBtn.addEventListener('click', () => {
      const form = document.getElementById('createTrainForm');
      if (!form.checkValidity()) {
        form.reportValidity();
        return;
      }

      const data = Object.fromEntries(new FormData(form).entries());
      data.IsActive = data.IsActive === 'true';

      fetch('/Admin/CreateTrain', {
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
        .catch(err => alert(err.message || localization.failedToCreate));
    });
  }

  //#endregion
}
