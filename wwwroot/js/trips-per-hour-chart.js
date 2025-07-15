const whiteBackgroundPlugin = {
  beforeDraw: chart => {
    const ctx = chart.ctx;
    ctx.save();
    ctx.globalCompositeOperation = 'destination-over';
    ctx.fillStyle = 'rgba(255,255,255,0)';
    ctx.fillRect(0, 0, chart.width, chart.height);
    ctx.restore();
  }
};

function prepareDatasets(datasets) {
  return datasets.map(ds => {
    const newDs = { ...ds };
    newDs.tension = 0.2;
    newDs.fill = true;

    let baseColor = newDs.backgroundColor || newDs.borderColor || '#1976d2';

    if (typeof baseColor === 'string' && baseColor.startsWith('#')) {
      const hex = baseColor.replace('#', '');
      if (hex.length === 6) {
        const r = parseInt(hex.substring(0, 2), 16);
        const g = parseInt(hex.substring(2, 4), 16);
        const b = parseInt(hex.substring(4, 6), 16);
        newDs.backgroundColor = `rgba(${r},${g},${b},0.10)`;
      } else {
        newDs.backgroundColor = 'rgba(25,118,210,0.10)';
      }
    } else if (typeof baseColor === 'string' && baseColor.startsWith('rgb')) {
      newDs.backgroundColor = baseColor.replace(/rgba?\(([^)]+)\)/, (match, colorValues) => {
        const parts = colorValues.split(',').map(x => x.trim());
        if (parts.length === 3) {
          return `rgba(${parts[0]},${parts[1]},${parts[2]},0.10)`;
        }
        if (parts.length === 4) {
          return `rgba(${parts[0]},${parts[1]},${parts[2]},0.10)`;
        }
        return match;
      });
    } else {
      newDs.backgroundColor = 'rgba(25,118,210,0.10)';
    }

    return newDs;
  });
}

export function renderTripsPerHourChart(canvasId, chartData) {
  const canvas = document.getElementById(canvasId);
  if (!canvas || !chartData || !chartData.datasets || chartData.datasets.length === 0) {
    return;
  }

  const preparedData = {
    ...chartData,
    datasets: prepareDatasets(chartData.datasets)
  };

  const ctx = canvas.getContext('2d');
  new Chart(ctx, {
    type: 'line',
    data: preparedData,
    options: {
      responsive: true,
      maintainAspectRatio: false,
      scales: {
        x: {
          title: { display: true, text: 'Часы' },
          grid: { color: '#e0e0e0' },
          ticks: { color: '#222' }
        },
        y: {
          title: { display: true, text: 'Число поездок' },
          beginAtZero: true,
          grace: '10%',
          ticks: { precision: 0, color: '#222' },
          grid: { color: '#e0e0e0' }
        }
      },
      plugins: {
        legend: {
          position: 'bottom',
          labels: {
            color: '#222',
            font: { size: 14, weight: 'bold' }
          }
        },
        title: { display: false }
      }
    },
    plugins: [whiteBackgroundPlugin]
  });
}
