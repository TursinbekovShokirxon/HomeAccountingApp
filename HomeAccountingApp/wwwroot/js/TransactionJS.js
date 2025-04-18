document.addEventListener('DOMContentLoaded', function () {
    // Элементы DOM
    const monthFilter = document.getElementById('monthFilter');
    const typeFilter = document.getElementById('typeFilter');
    const transactionsTable = document.getElementById('transactionsTable');
    var itemCountSelector = document.getElementById('itemsCountSelector');
    const dropdown = document.getElementById('categoryDropdown');
    // Состояние
    let allTransactions = [];
    let allMonths = [];
    let itemsPerPage = 25;

    // Инициализация
    init();
    function init() {
        // Инициализация месяцев в фильтрации
        loadMonthsInTransaction();
        // Инициализация категорий в фильтрации
        loadCategories()
        // После загрузки категорий устанавливаем обработчики
        setupCategoryCheckboxHandlers();
        loadTransactions();
        EventListeners();
    }
    function EventListeners() {
        // Обработчики событий работает при изменении типа доход расход
        typeFilter.addEventListener('change', () => {
            loadCategories(); // Обновляем категории при изменении типа
            loadTransactions(); // Загружаем транзакции с новыми фильтрами
        });

        // Обработчики событий работает при изменении месяца
        monthFilter.addEventListener('change', loadTransactions);

        // Обработчики событий работает при изменении выводимого количества элементов внизу слева
        itemCountSelector.addEventListener('change', (e) => {
            itemsPerPage = parseInt(e.target.value);
            loadTransactions(); // перезагружаем с новым лимитом
        });
    }



    function setupCategoryCheckboxHandlers() {
        dropdown.addEventListener('change', function (e) {
            if (!e.target.classList.contains('category-checkbox')) return;

            const allCheckbox = document.getElementById('catAll');
            const checkboxes = dropdown.querySelectorAll('.category-checkbox');

            // Обработка выбора "Все категории"
            if (e.target === allCheckbox) {
                if (e.target.checked) {
                    checkboxes.forEach(cb => {
                        if (cb !== allCheckbox) cb.checked = false;
                    });
                }
            } else {
                // Если выбрана конкретная категория
                allCheckbox.checked = false;

                // Если ничего не выбрано, автоматически выбираем "Все"
                const anyChecked = Array.from(checkboxes)
                    .some(cb => cb.checked && cb !== allCheckbox);

                if (!anyChecked) {
                    allCheckbox.checked = true;
                }
            }

            loadTransactions();
        });
    }

    async function loadCategories() {

        // Загружаем категории в зависимости от типа
        const selectedType = typeFilter.value;
        let selectedTypeString = "";
        if (selectedType === 'all') selectedTypeString = "/by-type";
        else selectedTypeString = `/by-type?type=${selectedType}`;


        await fetch(selectedTypeString, {
            cache: 'no-cache',
            method: 'GET',
            credentials: 'include'
        })
            .then(res => res.json())
            .then(data => {
                dropdown.innerHTML = ''; // Очистка перед новой вставкой
                dropdown.insertAdjacentHTML('beforeend', `
    <li>
        <div class="form-check">
            <input class="form-check-input category-checkbox" type="checkbox" value="all" id="catAll" checked>
            <label class="form-check-label" for="catAll">Все категории</label>
        </div>
    </li>
`);
                data.forEach(cat => {

                    const id = `cat${cat.id}`;
                    dropdown.insertAdjacentHTML('beforeend', `
                    <li>
                        <div class="form-check">
                            <input class="form-check-input category-checkbox" type="checkbox" value="${cat.id}" id="${id}">
                            <label class="form-check-label" for="${id}">${cat.name}</label>
                        </div>
                    </li>
                `);
                });
                dropdown.insertAdjacentHTML('beforeend', `
    <li>
        <div class="form-check">
            <input class="form-check-input category-checkbox" type="checkbox" value="other" id="other">
            <label class="form-check-label" for="other">Другое</label>
        </div>
    </li>
`);
                setupCategoryCheckboxHandlers();
            });
    }
    function GetInformationInCheckBox() {
        const checkboxes = dropdown.querySelectorAll('.category-checkbox');
        return Array.from(checkboxes)
            .filter(cb => cb.checked && cb.value !== 'all')
            .map(cb => {
                if (cb.value === 'other') return -1; // или другое специальное значение
                return parseInt(cb.value);
            });
    }
        // Основная функция загрузки транзакций с фильтрами
        async function loadTransactions() {
            try {
                const checkboxes = document.querySelectorAll('.category-checkbox');
                const selectedCategories = GetInformationInCheckBox();

                console.warn("Все категории ",checkboxes)
                console.warn("Все выбранные категории ",selectedCategories)

                // Формируем параметры запроса
                const params = new URLSearchParams();
                if (monthFilter.value !== 'all') params.append('month', monthFilter.value);
                if (typeFilter.value !== 'all') params.append('type', typeFilter.value);
                // Если выбраны конкретные категории (не "Все")
                if (!document.getElementById('catAll').checked && selectedCategories.length > 0) 
                    selectedCategories.forEach(cat => params.append('categories', cat));
                

                params.append("limit", itemsPerPage);
                const response = await fetch(`/api/transactions?${params.toString()}`, {
                    method: 'GET',
                    credentials: 'include'
                    , headers: {
                        'Content-Type': 'application/json'
                    }
                });

                if (!response.ok) 
                    throw new Error('Ошибка при загрузке транзакций');
                
                const data = await response.json();
                console.log("Загруженные транзакции:", data); // <- добавь
                allTransactions = data;
                renderTransactions();
            } catch (error) {
                console.error('Ошибка загрузки транзакций:', error);
                showError('Не удалось загрузить транзакции');
            }
        }
        async function loadMonthsInTransaction() {
            try {
                const response = await fetch('/api/months', {
                    method: 'GET',
                    cache: 'no-store',
                    credentials: 'include'
                });


            if (!response.ok) throw new Error('Ошибка загрузки месяцев');

                allMonths = await response.json();

                if (Array.isArray(allMonths) && allMonths.length === 0) {
                    console.warn('Месяцы не найдены, загружаем все транзакции');
                    loadTransactions(); // своя функция для загрузки всех данных
                    return;
                }
                updateMonthFilter();
            } catch (error) {
                console.error('Ошибка загрузки месяцев:', error);
                showError('Не удалось загрузить месяцы');
            }
        }
        function updateMonthFilter() {
            const monthFilter = document.getElementById('monthFilter');

            if (!monthFilter) {
                console.warn('Элемент monthFilter не найден');
                return;
            }
            monthFilter.innerHTML = '<option value="all">Все месяцы</option>';

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