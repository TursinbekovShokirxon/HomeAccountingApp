document.addEventListener('DOMContentLoaded', function () {
    // Элементы DOM
    const monthFilter = document.getElementById('monthFilter');
    const typeFilter = document.getElementById('typeFilter');
    const categoryFilter = document.getElementById('categoryFilter');
    const transactionsTable = document.getElementById('transactionsTable');
    var itemCountSelector = document.getElementById('itemsCountSelector');

 

    // Состояние
    let allTransactions = [];
    let allMonths = [];
    let allCategories = [];
    let itemsPerPage = 25;

    // Инициализация
    loadMonths();
    loadCategories();
    loadTransactions();

    // Обработчики событий
    typeFilter.addEventListener('change', () => {
        loadCategories(); // Обновляем категории при изменении типа
        loadTransactions(); // Загружаем транзакции с новыми фильтрами
    });
    monthFilter.addEventListener('change', loadTransactions);
    itemCountSelector.addEventListener('change', (e) => {
        itemsPerPage = parseInt(e.target.value);
        loadTransactions(); // перезагружаем с новым лимитом
    });
    categoryFilter.addEventListener('change', loadTransactions);


    // Загрузка категорий
    async function loadCategories() {
        try {
            categoryFilter.innerHTML = '<option value="all">Загрузка категорий...</option>';

            const selectedType = typeFilter.value;
            let type = 'all';

            if (selectedType === 'income') type = '1';
            else if (selectedType === 'expense') type = '0';
            const url = selectedType === 'all'
                ? '/by-type'
                : `/by-type?type=${type}`;

            const response = await fetch(url, {
                method: 'GET',
                credentials: 'include',
                cache: 'no-store'
            });

            if (!response.ok) throw new Error('Ошибка загрузки категорий');

            allCategories = await response.json();
            updateCategoryFilter();
        } catch (error) {
            console.error('Ошибка загрузки категорий:', error);
            showError('Не удалось загрузить категории');
            categoryFilter.innerHTML = '<option value="all">Ошибка загрузки</option>';
        }
    }
    function updateCategoryFilter() {
        categoryFilter.innerHTML = '<option value="all">Все категории</option>';
        const selectedType = typeFilter.value;

        allCategories
            .filter(cat => selectedType === 'all' ||
                (selectedType === 'income' && cat.type === 1) ||
                (selectedType === 'expense' && cat.type === 0))
            .forEach(cat => {
                const option = document.createElement('option');
                option.value = cat.id;
                option.textContent = cat.name;
                categoryFilter.appendChild(option);
            });
    }

    // Загрузка месяцев
    async function loadMonths() {
        try {
            const response = await fetch('/api/months', {
                method: 'GET',
                credentials: 'include'
            });

            if (!response.ok) throw new Error('Ошибка загрузки месяцев');

            allMonths = await response.json();
            updateMonthFilter();
        } catch (error) {
            console.error('Ошибка загрузки месяцев:', error);
            showError('Не удалось загрузить месяцы');
        }
    }
    // Обновление фильтра месяцев
    function updateMonthFilter() {
        monthFilter.innerHTML = '<option value="all">Все месяцы</option>';

        const monthNames = ['Январь', 'Февраль', 'Март', 'Апрель', 'Май', 'Июнь',
            'Июль', 'Август', 'Сентябрь', 'Октябрь', 'Ноябрь', 'Декабрь'];

        // Сортируем месяцы от новых к старым
        allMonths.sort().reverse().forEach(monthKey => {
            const [year, month] = monthKey.split('-');
            const option = document.createElement('option');
            option.value = monthKey;
            option.textContent = `${monthNames[parseInt(month) - 1]} ${year}`;
            monthFilter.appendChild(option);
        });
    }


    // Основная функция загрузки транзакций с фильтрами
    async function loadTransactions() {
        try {
            // Формируем параметры запроса
            const params = new URLSearchParams();
            if (monthFilter.value !== 'all') params.append('month', monthFilter.value);
            if (typeFilter.value !== 'all') params.append('type', typeFilter.value);
            if (categoryFilter.value !== 'all') params.append('category', categoryFilter.value);

            params.append("limit", itemsPerPage);
            const response = await fetch(`/api/transactions?${params.toString()}`, {
                method: 'GET',
                credentials: 'include'
                , headers: {
                    'Content-Type': 'application/json'
                }
            }
            );

            if (!response.ok) {
                throw new Error('Ошибка при загрузке транзакций');
            }
            const data = await response.json();
            console.log("Загруженные транзакции:", data); // <- добавь
            allTransactions = data;
            renderTransactions();
        } catch (error) {
            console.error('Ошибка загрузки транзакций:', error);
            showError('Не удалось загрузить транзакции');
        }
    }

    // Рендер таблицы транзакций
    function renderTransactions() {

        if (!Array.isArray(allTransactions)) {
            console.error("renderTransactions: ожидался массив, но пришло:", transactions);
            return;
        }
        if (allTransactions.length === 0) {
            transactionsTable.innerHTML = `
                <tr>
                    <td colspan="6" class="text-center py-5">
                        <i class="bi bi-file-earmark-text fs-1 text-muted"></i>
                        <p class="mt-2">Нет данных для отображения</p>
                    </td>
                </tr>
            `;
            return;
        }

        transactionsTable.innerHTML = allTransactions.map(transaction => `
            <tr class="${transaction.type === 1 ? 'table-success' : 'table-danger'}">
                <td>${formatDate(transaction.date)}</td>
                <td>${transaction.type === 1 ? 'Доход' : 'Расход'}</td>
                <td>${transaction.categoryName}</td>
                <td>${transaction.amount.toLocaleString()} сум</td>
                <td>${transaction.comment || '-'}</td>
                <td class="text-end">
                    <button class="btn btn-sm btn-outline-danger delete-btn" data-id="${transaction.id}">
                        <i class="bi bi-trash"></i>
                    </button>
                </td>
            </tr>
        `).join('');

        // Обработчики для кнопок удаления
        document.querySelectorAll('.delete-btn').forEach(btn => {
            btn.addEventListener('click', async () => {
                if (confirm('Удалить эту транзакцию?')) {
                    await deleteTransaction(btn.dataset.id);
                }
            });
        });
    }

    // Удаление транзакции
    async function deleteTransaction(id) {
        try {
            const response = await fetch(`/api/delete/transactions?id=${id}`, {
                method: 'DELETE',
                credentials: 'include'
            });

            if (response.ok) {
                loadTransactions(); // Перезагружаем данные после удаления
                showSuccess('Транзакция удалена');
            }
        } catch (error) {
            console.error('Ошибка удаления:', error);
            showError('Не удалось удалить транзакцию');
        }
    }

    // Вспомогательные функции
    function formatDate(dateString) {
        const date = new Date(dateString);
        return date.toLocaleDateString('ru-RU');
    }

    function showSuccess(message) {
        alert(message); // Можно заменить на toast-уведомление
    }

    function showError(message) {
        alert(message); // Можно заменить на toast-уведомление
    }
});