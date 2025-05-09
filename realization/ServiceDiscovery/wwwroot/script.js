const AREA = 'weatherforecast';
const API_BASE = `/services/${AREA}`;
const CORRELATION_ID = crypto.randomUUID();

async function fetchServices() {
    try {
        const response = await fetch(API_BASE, {
            headers: {
                'X-Correlation-Id': CORRELATION_ID
            }
        });

        if (!response.ok) throw new Error(await response.text());

        const data = await response.json();
        hideError(); 
        return data.services || [];
    } catch (err) {
        showError(err.message);
        return [];
    }
}
async function unregisterService(id) {
    try {
        const response = await fetch(`/services/${id}`, {
            method: 'DELETE',
            headers: {
                'X-Correlation-Id': CORRELATION_ID
            }
        });

        if (!response.ok) throw new Error(await response.text());
    } catch (err) {
        alert(`Ошибка при отключении сервиса: ${err.message}`);
    }
}

async function registerService(service) {
    try {
        const requestBody = {
            ...service,
            area: AREA 
        };

        const response = await fetch(`/services`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-Correlation-Id': CORRELATION_ID
            },
            body: JSON.stringify(requestBody)
        });

        console.log(JSON.stringify(requestBody));

        if (!response.ok) throw new Error(await response.text());
    } catch (err) {
        alert(`Ошибка при включении сервиса: ${err.message}`);
    }
}

async function renderServices() {
    const tbody = document.getElementById('serviceList');
    tbody.innerHTML = '';

    const services = await fetchServices();

    services.forEach(service => {
        const row = document.createElement('tr');

        const nameCell = document.createElement('td');
        nameCell.textContent = `${service.host}:${service.port}:${service.id}`;
        row.appendChild(nameCell);

        const statusCell = document.createElement('td');
        statusCell.innerHTML = service.isHealthy
            ? '<span class="online">✅ Registered</span>'
            : '<span class="offline">❌ Unregistered</span>';
        row.appendChild(statusCell);

        const actionCell = document.createElement('td');
        const toggleBtn = document.createElement('button');
        toggleBtn.className = 'toggle';
        toggleBtn.textContent = service.isHealthy ? 'Unregister' : 'Register';

        toggleBtn.onclick = async () => {
            if (service.isHealthy) {
                await unregisterService(service.id);
            } else {
                await registerService(service);
            }
            await renderServices();
        };

        actionCell.appendChild(toggleBtn);
        row.appendChild(actionCell);

        tbody.appendChild(row);
    });
}

renderServices();

function showError(message) {
    const errorDiv = document.getElementById('errorMessage');
    errorDiv.textContent = message;
    errorDiv.style.display = 'block';

    document.getElementById('serviceTable').style.display = 'none';
}

function hideError() {
    const errorDiv = document.getElementById('errorMessage');
    errorDiv.style.display = 'none';

    document.getElementById('serviceTable').style.display = 'table';
}

