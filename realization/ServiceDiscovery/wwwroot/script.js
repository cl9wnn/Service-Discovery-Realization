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
        return data.services || [];
    } catch (err) {
        alert('Ошибка при загрузке сервисов: ' + err.message);
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
        alert(`Ошибка при удалении сервиса: ${err.message}`);
    }
}

async function renderServices() {
    const tbody = document.getElementById('serviceList');
    tbody.innerHTML = ''; 

    const services = await fetchServices();

    services.forEach(service => {
        const row = document.createElement('tr');

        const nameCell = document.createElement('td');
        nameCell.textContent = `${service.host}:${service.port}`;
        row.appendChild(nameCell);

        const statusCell = document.createElement('td');
        statusCell.innerHTML = `<span class="online">✅ Registered</span>`;
        row.appendChild(statusCell);

        const actionCell = document.createElement('td');
        const toggleBtn = document.createElement('button');
        toggleBtn.className = 'toggle unregister';
        toggleBtn.textContent = 'Unregister';

        toggleBtn.onclick = async () => {
            await unregisterService(service.id);
            await renderServices();
        };

        actionCell.appendChild(toggleBtn);
        row.appendChild(actionCell);

        tbody.appendChild(row);
    });
}

renderServices();
