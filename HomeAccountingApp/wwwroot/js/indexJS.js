document.addEventListener('DOMContentLoaded', function () {
    const incomeRadio = document.getElementById('incomeType');
    const expenseRadio = document.getElementById('expenseType');
    const categorySelect = document.getElementById('category');
    const transactionForm = document.getElementById('transactionForm');

    let categoriesCache = {};
    let balanceInMonth = 0;


    // Установка текущей даты по умолчанию
    const createdDateInput = document.getElementById('createdDate');
    if (createdDateInput) {
        const today = new Date();
        createdDateInput.value = today.toISOString().split('T')[0];
    }
    // Функция для загрузки категорий
    async function loadCategories(isIncome) {
        const type = isIncome ? 1 : 0;
        try {
            categorySelect.innerHTML = '<option value="" disabled selected>Загрузка категорий...</option>';

            const response = await fetch(`/by-type?type=${type}`, {
                credentials: 'include',
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) throw new Error('Ошибка загрузки категорий');

            const categories = await response.json();
            categoriesCache[type] = categories;
            renderCategories(categories, isIncome);

        } catch (error) {
            console.error('Ошибка:', error);
            showAlert('danger', 'Ошибка загрузки категорий');
            categorySelect.innerHTML = '<option value="" disabled selected>Ошибка загрузки</option>';
        }
    }

    // Отображение категорий в select
    function renderCategories(categories, isIncome) {
        categorySelect.innerHTML = '';

        // Опция по умолчанию
        const defaultOption = document.createElement('option');
        defaultOption.value = '';
        defaultOption.textContent = 'Выберите категорию';
        defaultOption.disabled = true;
        defaultOption.selected = true;
        categorySelect.appendChild(defaultOption);

        // Добавляем категории
        categories.forEach(cat => {
            const opt = document.createElement('option');
            opt.value = cat.id;
            opt.textContent = cat.name;
            categorySelect.appendChild(opt);
        });

        // Добавляем опцию "Другое"
        const otherOption = document.createElement('option');
        otherOption.value = 'other';
        otherOption.textContent = 'Другое';
        categorySelect.appendChild(otherOption);
    }

    // Обработчики изменения типа операции
    incomeRadio.addEventListener('change', () => loadCategories(true));
    expenseRadio.addEventListener('change', () => loadCategories(false));

    // Обработчик отправки формы
    transactionForm.addEventListener('submit', async function (e) {
        e.preventDefault();

        // Валидация суммы
        const amount = parseFloat(document.getElementById('amount').value);

        if (isNaN(amount) || amount <= 0) {
            showAlert('danger', 'Введите корректную сумму');
            return;
        }

        
        // Получаем данные формы
        const isIncome = document.querySelector('input[name="transactionType"]:checked').value === 'income';
        const categoryValue = categorySelect.value;
        const dateValue = document.getElementById('createdDate').value;

        // Проверка выбора категории
        if (!categoryValue) {
            showAlert('danger', 'Выберите категорию');
            return;
        }


        // Определяем ID категории
        let categoryId;
        if (categoryValue === 'other') 
            categoryId = isIncome ? 10 : 9; // ID для "Другое"
        else 
            categoryId = parseInt(categoryValue);

        // Проверка даты
        if (!dateValue) {
            showAlert('danger', 'Укажите дату');
            return;
        }
        //Проверка хватает ли средств на балансе
        if (!isIncome) {
            console.log(dateValue);
            const dateValueCopy = new Date(dateValue);
            console.log(dateValueCopy);
            const year = dateValueCopy.getFullYear();
            const month = String(dateValueCopy.getMonth() + 1).padStart(2, '0');
            const formattedDate = `${year}-${month}`;
            console.log("дата : ",formattedDate)
            balanceInMonth = await fetch(`/api/statistics?month=${formattedDate}`, {
                cache: 'no-store',
                method: 'GET',
                credentials: 'include'
            })
            if (!balanceInMonth.ok) {
                showAlert('danger', 'Укажите дату');
                throw new Error('Ошибка загрузки статистики');
            }

            const stats = await balanceInMonth.json();

            if ((stats.balance - amount) < 0) {

                showAlert('danger', "У вас на этом месяце не хватает на балансе");
                return;
            }

        }

        const formData = {
            IsIncome: isIncome,
            CategoryId: categoryId,
            Ammount: amount,
            Comment: document.getElementById('comment').value,
            Date: dateValue
        };

        try {
            const response = await fetch('/add-transaction', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                credentials: 'include',
                body: JSON.stringify(formData)
            });
            console.log(formData);
            loadMonths();
            loadStatistics();
            if (!response.ok) {
                const errorData = await response.json().catch(() => ({}));
                throw new Error(errorData.message || 'Ошибка при добавлении транзакции');
            }

            showAlert('success', 'Транзакция успешно добавлена');
            transactionForm.reset();
            // Установка текущей даты по умолчанию
            const createdDateInput = document.getElementById('createdDate');
            if (createdDateInput) {
                const today = new Date();
                createdDateInput.value = today.toISOString().split('T')[0];
            }

            // Восстанавливаем выбранный тип операции
            if (isIncome) {
                incomeRadio.checked = true;
                loadCategories(true);
            } else {
                expenseRadio.checked = true;
                loadCategories(false);
            }

            loadStatistics();

        } catch (error) {
            console.error('Ошибка:', error);
            showAlert('danger', error.message);
        }
    });

    // Функция показа уведомлений
    function showAlert(type, message) {
        const alert = document.createElement('div');
        alert.className = `alert alert-${type} alert-dismissible fade show`;
        alert.role = 'alert';
        alert.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        `;

        const container = document.querySelector('.container');
        container.prepend(alert);

        setTimeout(() => {
            alert.classList.remove('show');
            setTimeout(() => alert.remove(), 150);
        }, 3000);
    }

    // Инициализация
    loadCategories(incomeRadio.checked);
});