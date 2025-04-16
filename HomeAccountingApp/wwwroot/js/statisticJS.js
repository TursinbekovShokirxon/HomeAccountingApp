let allMonths = [];
const monthStatFilter = document.getElementById('monthStatFilter');
document.addEventListener('DOMContentLoaded', () => {
    loadMonths();
    monthStatFilter.addEventListener('change', loadStatistics);
});

monthStatFilter.addEventListener('change', loadStatistics);
async function loadStatistics() {
    const selectedMonth = document.getElementById('monthStatFilter').value;

    try {
        const response = await fetch(`/api/statistics?month=${selectedMonth}`, {
            method: 'GET',
            credentials: 'include'
        });

        if (!response.ok) throw new Error('Ошибка загрузки статистики');

        const stats = await response.json();
        document.getElementById('totalIncome').textContent = `${stats.totalIncome.toLocaleString()} сум`;
        document.getElementById('totalExpense').textContent = `${stats.totalExpense.toLocaleString()} сум`;
        document.getElementById('balance').textContent = `${stats.balance.toLocaleString()} сум`;
    } catch (error) {
        console.error('Ошибка загрузки статистики:', error);
        showError('Не удалось загрузить статистику');
    }
}

async function loadMonths() {
    try {
        const response = await fetch('/api/months', {
            method: 'GET',
            credentials: 'include'
        });

        if (!response.ok) throw new Error('Ошибка загрузки месяцев');

        allMonths = await response.json();
        updateMonthFilter();
        loadStatistics(); // после установки месяцев сразу грузим статистику за текущий
    } catch (error) {
        console.error('Ошибка загрузки месяцев:', error);
        showError('Не удалось загрузить месяцы');
    }
}
function updateMonthFilter() {
    const monthFilter = document.getElementById('monthStatFilter');
    monthFilter.innerHTML = ''; // Очищаем перед заполнением

    const monthNames = ['Январь', 'Февраль', 'Март', 'Апрель', 'Май', 'Июнь',
        'Июль', 'Август', 'Сентябрь', 'Октябрь', 'Ноябрь', 'Декабрь'];

    // Сортировка месяцев от нового к старому
    allMonths.sort().reverse().forEach(monthKey => {
        const [year, month] = monthKey.split('-');
        const option = document.createElement('option');
        option.value = monthKey;
        option.textContent = `${monthNames[parseInt(month) - 1]} ${year}`;
        monthFilter.appendChild(option);
    });

    // Устанавливаем текущий месяц, если есть в списке
    const current = new Date().toISOString().slice(0, 7);
    if (allMonths.includes(current)) {
        monthFilter.value = current;
    } else if (allMonths.length > 0) {
        monthFilter.value = allMonths[0]; // если текущего нет, выбираем самый последний
    }
}
