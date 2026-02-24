using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

[System.Serializable]
public class Question
{
    [TextArea] public string questionText;
    public string redOption;
    public string yellowOption;
    public string greenOption;
    public string blueOption;

    public Question(string text, string red, string yellow, string green, string blue)
    {
        questionText = text;
        redOption = red;
        yellowOption = yellow;
        greenOption = green;
        blueOption = blue;
    }
}

public class ProfessionTestManager : MonoBehaviour
{
    [Header("UI Text References")]
    public TMP_Text mainQuestionText;
    public TMP_Text AnswerRedText;
    public TMP_Text AnswerYellowText;
    public TMP_Text AnswerGreenText;
    public TMP_Text AnswerBlueText;

    [Header("Teleportation Settings")]
    public GameObject playerObject; 

    // Вписал твои координаты здесь:
    private Dictionary<string, Vector3> _professionCoordinates = new Dictionary<string, Vector3>()
    {
        {"Программист", new Vector3(3.37463f, 1.11132f, -24.94623f)}, 
        {"Учитель",     new Vector3(42.258f, 1.311f, -13.10123f)},
        {"Медик",       new Vector3(-5.4f, 1.45f, 92.08f)},
        {"Судья",       new Vector3(11f, 2.648f,121.14f)},
        {"Дизайнер",    new Vector3(-10.93f, 1.99f, 147.3f)},
        {"Психолог",    new Vector3(-6.362f, 3.46f, 62.952f)}
    };

    [Header("Settings")]
    public List<Question> questions = new List<Question>();
    
    private int _currentIndex = 0;
    private Dictionary<string, float> _scores = new Dictionary<string, float>()
    {
        {"Программист", 0f}, {"Учитель", 0f}, {"Медик", 0f},
        {"Дизайнер", 0f}, {"Судья", 0f}, {"Психолог", 0f}
    };

    void Awake()
    {
        FillQuestionsData();
    }

    void Start()
    {
        if (questions.Count > 0) 
            UpdateUI();
        else 
            Debug.LogError("Вопросы не загружены!");
    }

    public void AnswerChosen(string color)
    {
        if (_currentIndex >= questions.Count) return;

        ApplyMultiClassWeights(color);
        _currentIndex++;

        if (_currentIndex < questions.Count)
            UpdateUI();
        else
            ShowResult();
    }

    private void ApplyMultiClassWeights(string color)
    {
        switch (color)
        {
            case "Red":
                _scores["Программист"] += 1.0f;
                _scores["Дизайнер"] += 0.6f;
                break;
            case "Yellow":
                _scores["Судья"] += 1.0f;
                _scores["Программист"] += 0.3f;
                break;
            case "Green":
                _scores["Медик"] += 1.0f;
                _scores["Психолог"] += 0.5f;
                break;
            case "Blue":
                _scores["Дизайнер"] += 0.8f;
                _scores["Учитель"] += 0.7f;
                _scores["Психолог"] += 0.7f;
                break;
        }
    }

    void UpdateUI()
    {
        Question q = questions[_currentIndex];
        mainQuestionText.text = $"<size=60%><color=#AAAAAA>Вопрос {_currentIndex + 1} из {questions.Count}</color></size>\n\n{q.questionText}";
        
        AnswerRedText.text = q.redOption;
        AnswerYellowText.text = q.yellowOption;
        AnswerGreenText.text = q.greenOption;
        AnswerBlueText.text = q.blueOption;
    }

    void ShowResult()
    {
        var sortedResults = _scores.OrderByDescending(x => x.Value).ToList();
        string topProfession = sortedResults[0].Key;

        mainQuestionText.text = $"<color=yellow>Анализ завершен!</color>\n\n" +
                               $"Подходящая роль: <b>{topProfession}</b>\n" +
                               $"Вы будете перемещены в кабинет через 3 секунды...";
        
        HideAnswers();
        StartCoroutine(TeleportProcess(topProfession));
    }

    private IEnumerator TeleportProcess(string profession)
    {
        yield return new WaitForSeconds(3f);

        if (playerObject != null && _professionCoordinates.ContainsKey(profession))
        {
            Vector3 targetPosition = _professionCoordinates[profession];

            // Отключаем контроллер, чтобы физика не мешала телепортации
            CharacterController cc = playerObject.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            playerObject.transform.position = targetPosition;
            Debug.Log($"[УСПЕХ]: Телепортация в {profession} на {targetPosition}");

            if (cc != null) cc.enabled = true;
        }
        else
        {
            Debug.LogError("PlayerObject не назначен в инспекторе!");
        }
    }

    private void HideAnswers()
    {
        if (AnswerRedText != null) AnswerRedText.transform.parent.gameObject.SetActive(false);
        if (AnswerYellowText != null) AnswerYellowText.transform.parent.gameObject.SetActive(false);
        if (AnswerGreenText != null) AnswerGreenText.transform.parent.gameObject.SetActive(false);
        if (AnswerBlueText != null) AnswerBlueText.transform.parent.gameObject.SetActive(false);
    }

    void FillQuestionsData()
    {
        questions.Clear();
        // БЛОК 1: Быт и Техника
        questions.Add(new Question("Что сделаешь первым с новым гаджетом?", "Буду копаться в настройках", "Прочитаю инструкцию", "Проверю безопасность", "Настрою красивые обои"));
        questions.Add(new Question("Сломался компьютер в классе. Твоя реакция?", "Попробую найти причину", "Позову учителя", "Проверю, не перегрелся ли он", "Буду общаться с друзьями"));
        questions.Add(new Question("Твой идеальный рюкзак это...", "Много отделов для кабелей", "Прочный и классический", "Удобный и экологичный", "Самый стильный и яркий"));
        questions.Add(new Question("Если техника работает медленно, ты...", "Попытаюсь оптимизировать", "Буду строго следовать инструкции", "Проверю, не вредит ли это работе", "Найду более красивое решение"));
        questions.Add(new Question("В компьютерных играх ты скорее...", "Инженер или создатель", "Командир, следящий за правилами", "Медик, спасающий команду", "Персонаж с самым крутым скином"));

        // БЛОК 2: Люди и Помощь
        questions.Add(new Question("Друг сильно расстроен. Как поможешь?", "Дам четкий план действий", "Разберусь, кто его обидел", "Принесу чай и плед", "Выслушаю и подбодрю"));
        questions.Add(new Question("Нашел котенка на улице. Что сделаешь?", "Выложу объявление в сеть", "Позвоню в службу спасения", "Сразу отвезу к ветеринару", "Накормлю и приласкаю"));
        questions.Add(new Question("Тебе нужно объяснить тему однокласснику:", "Нарисую логическую схему", "Процитирую учебник", "Приведу пример из жизни", "Расскажу яркую историю"));
        questions.Add(new Question("В командном проекте ты...", "Тот, кто делает техническую часть", "Тот, кто следит за сроками", "Тот, кто мирит всех", "Тот, кто оформляет презентацию"));
        questions.Add(new Question("Что самое важное в общении?", "Логика и факты", "Честность и правила", "Забота и поддержка", "Вдохновение и красота"));

        // БЛОК 3: Школа и Интересы
        questions.Add(new Question("Какой школьный проект интереснее?", "Создание сайта/программы", "Доклад о правах человека", "Исследование о здоровье", "Выставка рисунков/фото"));
        questions.Add(new Question("В музее ты дольше простоишь у...", "Первых роботов и моторов", "Древних сводов законов", "Экспозиции о теле человека", "Картины великого мастера"));
        questions.Add(new Question("Любимый урок в расписании?", "Информатика / Математика", "История / Право", "Биология / Химия", "ИЗО / Литература"));
        questions.Add(new Question("Как ты готовишься к экзаменам?", "Использую нейросети и софт", "Строго по списку тем", "Слежу за режимом сна и питания", "Рисую яркие схемы-карты"));
        questions.Add(new Question("Если бы ты писал книгу, она была бы о...", "Будущем и технологиях", "Поиске справедливости", "Подвигах врачей", "Человеческих чувствах"));

        // БЛОК 4: Ситуации
        questions.Add(new Question("Потерялся в городе. Что предпримешь?", "Открою карты в телефоне", "Найду официальный пост полиции", "Зайду в аптеку за советом", "Спрошу дорогу у прохожих"));
        questions.Add(new Question("Тебе доверили украсить зал. С чего начнешь?", "Настрою освещение и звук", "Составлю смету и план", "Проверю, чтобы всем было комфортно", "Выберу стиль и палитру цветов"));
        questions.Add(new Question("Если видишь несправедливость, ты...", "Считаю это системной ошибкой", "Буду требовать наказания", "Помогу пострадавшему", "Попробую всех помирить"));
        questions.Add(new Question("Как ты принимаешь важные решения?", "Анализирую данные и цифры", "Опираюсь на правила и мораль", "Слушаю интуицию и чувства", "Ищу самое необычное решение"));
        questions.Add(new Question("Твое отношение к спорам:", "Спорю только цифрами и фактами", "Спорю, если нарушен порядок", "Избегаю их, это портит нервы", "Пытаюсь понять обе стороны"));

        // БЛОК 5: Взгляд в будущее
        questions.Add(new Question("Какую суперсилу ты бы выбрал?", "Управлять техникой мыслью", "Видеть ложь насквозь", "Мгновенно исцелять", "Создавать миры воображением"));
        questions.Add(new Question("Твой идеальный дом будущего?", "Умный дом с роботами", "Надежный и безопасный дом", "Эко-дом в лесу", "Дом с уникальным дизайном"));
        questions.Add(new Question("Чем бы ты занялся в космосе?", "Настройкой бортовых систем", "Созданием космического права", "Изучением инопланетной жизни", "Оформлением жилых модулей"));
        questions.Add(new Question("Что важнее всего в людях?", "Интеллект", "Честность", "Доброта", "Творчество"));
        questions.Add(new Question("В свободное время ты скорее...", "Будешь изучать новый софт", "Посмотришь детектив", "Займешься спортом/здоровьем", "Будешь творить или рисовать"));
        questions.Add(new Question("Мир будущего — это мир...", "Высоких технологий", "Идеального закона", "Победивших болезней", "Сплошного искусства"));
        questions.Add(new Question("Если бы ты стал ученым, ты бы...", "Создал искусственный разум", "Доказал важную историческую правду", "Нашел лекарство от всех вирусов", "Изучал психологию творчества"));
        questions.Add(new Question("Что тебя вдохновляет?", "Сложные механизмы", "Торжество справедливости", "Спасенная жизнь", "Красивый вид или картина"));
        questions.Add(new Question("Твое отношение к дисциплине:", "Это способ быть эффективнее", "Это основа любого порядка", "Важна, но здоровье важнее", "Главное — чтобы она не мешала идеям"));
        questions.Add(new Question("Почему этот тест важен для тебя?", "Интересно узнать алгоритм", "Хочу получить объективный ответ", "Хочу понять, как помогать людям", "Просто интересный опыт"));
    }
}