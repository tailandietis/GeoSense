# GeoSense — Платформа интеллектуального мониторинга геофизических событий

Дипломная работа: «Разработка платформы интеллектуального мониторинга и анализа геофизических событий на основе методов машинного обучения»

---

## О проекте

**GeoSense** — программная платформа, расширяющая возможности системы сейсмического мониторинга GeoDa методами машинного обучения. Система оценивает текущий уровень сейсмической опасности в режиме реального времени, классифицируя события по 4 уровням риска с помощью алгоритма XGBoost.

### Уровни риска

| Класс | Название | Описание |
|-------|----------|----------|
| 0 | Фон | Щелчки породы, нормальная активность |
| 1 | Умеренный | Начальные признаки повышенной активности |
| 2 | Высокий | Пучение почвы, давление на борт выработки |
| 3 | Критический | Горный удар, критическая нагрузка на массив |

### Ключевые возможности

- **ML Dashboard** — обучение XGBoost и Isolation Forest, метрики, сравнение с пороговыми методами (baseline), анализ устойчивости к погрешностям датчиков
- **ML Классификация** — классификация сейсмических событий с цветовой индикацией риска
- **ML vs GeoDa** — сравнение ML-оценки с детерминированным методом параметра F
- **3D Карта событий** — пространственная визуализация сейсмической активности
- **Региональный прогноз** — интеграция с модулем GeoDa для расчёта параметра F

---

## Архитектура

```
GeoSense/
├── mlbuilder/              # Python ML компонент
│   ├── ml_builder.py       # Точка входа (--mode train / predict)
│   ├── emulator.py         # Генератор синтетических событий
│   ├── feature_engineering.py  # 23 признака
│   ├── train.py            # XGBoost + Isolation Forest + baseline
│   ├── predict.py          # Классификация новых событий
│   └── settings/           # Конфигурация
└── src/
    ├── GeoSense.Domain/    # Доменные модели
    ├── GeoDa.Application/  # Бизнес-логика
    ├── GeoDa.Database/     # Репозитории PostgreSQL
    ├── GeoDa.Infrastructure/  # Инфраструктура
    └── GeoDa.BlazorWebApp/ # Blazor Server веб-приложение
```

### Стек технологий

| Компонент | Технология |
|-----------|------------|
| Веб-приложение | .NET 6, Blazor Server |
| UI компоненты | AntDesign Blazor 0.12 |
| База данных | PostgreSQL |
| ML | Python 3.10+, XGBoost, scikit-learn |
| PDF отчёты | QuestPDF |

---

## Требования

### Для веб-приложения
- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- PostgreSQL 14+ (опционально — без БД работает ML-часть)

### Для ML компонента
- Python 3.10+
- Зависимости из `mlbuilder/requirements.txt`

---

## Развёртывание

### 1. Клонирование репозитория

```bash
git clone https://github.com/ВАШ_ЛОГИН/GeoSense.git
cd GeoSense
```

### 2. Настройка ML компонента

```bash
cd mlbuilder

# Создать виртуальное окружение
python -m venv venv

# Активировать (Windows)
venv\Scripts\activate

# Установить зависимости
pip install -r requirements.txt
```

Проверить работу:
```bash
python ml_builder.py --mode train --output output
```

После обучения в папке `output/` появятся:
- `xgb_model.json` — обученная модель
- `iso_forest.pkl` — Isolation Forest
- `metrics.json` — метрики качества
- `demo_events.csv` — демо-события для predict
- `plots/` — графики (confusion matrix, feature importance и др.)

### 3. Настройка веб-приложения

Отредактировать `src/GeoDa.BlazorWebApp/appsettings.json`:

**Подключение к БД** (если есть):
```json
"DbsSettings": {
  "ConnectionSettings": {
    "DbPeleng": {
      "ConnectionString": "Host=localhost;Port=5432;Database=pelengdb;Username=peleng",
      "Password": "ВАШ_ПАРОЛЬ"
    }
  }
}
```

**Путь к ML компоненту:**
```json
"MLBuilder": {
  "BuilderDir": "АБСОЛЮТНЫЙ_ПУТЬ\\mlbuilder",
  "OutputDir": "АБСОЛЮТНЫЙ_ПУТЬ\\mlbuilder\\output",
  "ThesisFiguresDir": ""
}
```

### 4. Запуск веб-приложения

```bash
cd src/GeoDa.BlazorWebApp
dotnet run
```

Приложение будет доступно по адресу: **https://localhost:5011**

---

## Использование без базы данных

Система полностью функциональна в части ML без подключения к БД:

1. Открыть **https://localhost:5011**
2. Перейти в **ML → ML Dashboard**
3. Нажать **«Обучить модель»** — запустится обучение на синтетических данных
4. После обучения перейти в **ML → ML Классификация**
5. Нажать **«Запустить классификацию»** — появится таблица событий с ML-классами

Модули регионального прогноза и работы с архивом требуют подключения к PostgreSQL БД системы GeoDa.

---

## ML компонент — подробнее

### Режимы запуска

```bash
# Обучение
python ml_builder.py --mode train --output ./output

# Классификация событий
python ml_builder.py --mode predict \
  --events events.csv \
  --model output/xgb_model.json \
  --iso_model output/iso_forest.pkl \
  --out output/predictions.json
```

### Формат входного CSV для predict

Обязательные колонки: `idat`, `itim`, `x`, `y`, `z`, `e`

| Колонка | Описание |
|---------|----------|
| idat | Дата события (юлианский день) |
| itim | Время события (секунды от начала дня) |
| x, y, z | Координаты гипоцентра (м) |
| e | Энергия события (Дж) |

### Признаки модели (23 шт.)

- Энергетические: `e`, `log_e`, `magn`, `e_sum_1h/6h/24h`, `energy_trend_6h`
- Пространственные: расстояние от центра, `dz`
- Временные: час, день недели, `count_1h/6h/24h`, `count_trend_6h`, нормированные счётчики
- Контекстные: `e_max_6h`, `e_mean_6h`, `e_std_6h`
