export default {
  apiError: {
    // Unhandled Error Page
    title: 'Что-то пошло не так!',
    ok: 'OK',
    serviceUnavailable: 'Сервис временно недоступен.',
  },

  validation: {
    title: 'Что-то пошло не так!',
    rules: {
      required: 'Поле {key} является обязательным к заполнению',
      email: 'В поле {key} должен быть адрес электронной почты',
      match: 'Поля {key} и {param} должны быть идентичны',
      passwordPolicy:
        '{key} должен содержать не менее 8 символов. В пароле должно быть хотя бы по одному символу следующих типов: большие латинские буквы, маленькие латинские буквы, цифры, спецсимволы.',
      usernamePolicy:
        '{key} должен быть от 4 до 12 символов в длину и может содержать только буквы латинского алфавита, цифры, подчёркивания, тире и апострофы (` _ -)',
    },
  },

  fp: {
    title: 'Главная',
    ladder: 'Доска почёта',
    community: 'Присоединяйтесь к нам в Discord',
    login: 'Войти',
    playNow: 'Регистрация',
    playWith: 'Нас уже {amount}. Сыграем?',
    register: 'Зарегистрироваться',
    playersOnline: 'Игроков в сети',
    registeredAccounts: 'Зарегистрированных игроков',
    matchesInProgress: 'Матчей в процессе',
    featuredStreamer: 'Популярный стрим',
    showFestivities: 'Show festivities',
    timeline: {
      title: 'История Project KONGOR',
      a: {
        date: '13 декабря 2021',
        title: 'Объявлена дата закрытия HoN',
        text: 'Frostburn официально объявляет - HoN будет закрыт 20 июня 2022.',
      },
      b: {
        date: '13 декабря 2021',
        title: 'Старт разработки Project KONGOR',
        text: 'Начинается сбор данных о движке игры, появляется первый тестовый билд, доказывающий принципиальную возможность реализации проекта. В первом билде можно было залогиниться и поиграть в плинко.',
      },
      c: {
        date: '20 июня 2022',
        title: 'Серверы HoN закрываются',
        text: 'Спустя 13 лет после релиза в далёком 2010 году, игра закрывается.',
      },
      d: {
        date: '5 августа 2022',
        title: 'Старт открытого бета-теста',
        text: 'Project KONGOR переходит в фазу открытого бета-тестирования и открывает свои двери для всех желающих.',
      },
      e: {
        date: '5 января 2023',
        title: 'Проект KONGOR достигает двух важных вех в своей истории',
        text: 'Установлен рекорд в 4700 игроков онлайн, а так же, сыграно более миллиона матчей с августа 2022!',
      },
    },
  },

  ladder: {
    title: 'Ладдер',
    gameModes: {
      caldavar: 'Caldavar',
      caldavarCasual: 'Caldavar (Casual)',
      midwars: 'Midwars',
    },
    subTitle: 'Топ-{x} игроков на карте {gameMode}',
    search: 'Поиск',
    mmr: '{rating} MMR',
    gamesPlayed: 'Матчей',
    winRate: 'Доля побед',
    KDA: 'K/D/A',
    GPM: 'GPM',
    APM: 'APM',
  },

  login: {
    // Login Page
    title: 'Вход',
    username: 'Логин',
    password: 'Пароль',
    passwordMatch: 'Повторите пароль',
    forgotPassword: 'Я забыл пароль',
    login: 'Войти',
    youMustBeAuthed: 'Эта страница только для зарегистрированных пользователей',
    youAreLoggedOut: 'Вы вышли из системы',
    emailAddress: 'Адрес электронной почты',
    forgotPasswordSubmit: 'Отправить данные для смены пароля на почту',
    changePasswordAction: 'Сменить пароль',
    passwordRequestResetSuccessTitle: 'Запрос смены пароля успешно отправлен',
    passwordRequestResetSuccessMessage:
      'На почту "{email}" отправлены дальнейшие инструкции',
    changePasswordTitle: 'Смена пароля',
    changePasswordSuccessTitle: 'Пароль изменён',
    changePasswordSuccessMessage: 'Ваш пароль был успешно изменён',
  },

  register: {
    // Registration Page
    title: 'Регистрация',
    username: 'Логин',
    password: 'Пароль',
    passwordMatch: 'Повторите пароль',
    emailAddress: 'Адрес электронной почты',
    emailAddressMatch: 'Повторите адрес электронной почты',
    registerAction: 'Зарегистрироваться',
    registerSuccessTitle: 'Email зарегистрирован',
    registerSuccessfulMessage:
      'На почту "{email}" отправлены дальнейшие инструкции',
    completeRegisterAction: 'Завершить регистрацию',
    completeYourRegistrationTitle: 'Завершение регистрации',
    registerCompleteSuccessTitle: 'Учётная запись создана',
    registerCompleteSuccessMessage:
      'Есть сложности со скачиванием и установкой HoN? Заходи на наш сервер в Discord. Там ты можешь задать свои вопросы сообществу и получить помощь.',
    joinOurDiscord: 'Присоединяйтесь к нам в Discord',
  },

  dashboard: {
    // Account Page
    title: 'Личный кабинет',
    mySettingsBtn: 'Настройки учётной записи',
    logout: 'Выйти',
    welcome: 'Привет, {name}!',
    menuTitles: {
      gmTools: 'Инструменты ГМа',
      sgmTools: 'Инструменты старшего ГМа',
      adminTools: 'Administrator tools',
    },
    youAreNotAllowedHere:
      'Упс! Сюда нельзя! Если думаешь, что это по ошибке, сообщи администратору.',

    mySettings: {
      // Account Settings Sub-Page
      title: 'Настройки учётной записи',

      changeEmail: {
        // Update Email Tab
        title: 'Сменить адрес электронной почты',
        emailNotVerified: 'Электронная почта не подтверждена',
        emailVerified: 'Электронная почта подтверждена',
        emailAddress: 'Новый адрес',
        emailAddressMatch: 'Повторите новый адрес',
        changeEmailAction: 'Сменить адрес',
        password: 'Введите ваш пароль',
        changeRequestSuccessTitle: 'Запрос на смену адреса отправлен',
        changeRequestSuccessMessage:
          'На почту "{email}" отправлены дальнейшие инструкции',
        emailChangedSuccessfully:
          'Ваш адрес электронной почты был успешно изменён',
      },
    },

    cai: {
      title: 'Своя иконка аккаунта',
      selectFile: 'Нажми сюда, чтобы выбрать файл',
      instructions:
        'Используй левую клавишу и колёсико мыши, чтобы перетащить рамку или изменить её масштаб',
      uploadAction: 'Всё в порядке, загружаем!',
      uploadedWithSuccess:
        'Иконка загружена! Возможно, придётся перезагрузить игру, чтобы она появились в хранилище!',
    },

    gm: {
      suspension: {
        title: 'Управление наказаниями',
        lookup: 'Найти учётную запись',
        suspensionId: 'Бан №{id}',
        previousSuspensions: 'Предыдущие наказания ({count})',
        noBanMessage: 'Записи о наказаниях "{accountName}" не найдены.',
        accountName: 'Учётная запись',
        duration: 'Продолжительность',
        reason: 'Причина',
        internalDetails: 'Подробности (только для ГМов и администраторов)',
        submit: 'Отправить',
        suspensionSuccessful:
          'Данные отправлены, игроку "{accountName}" выдан бан на "{duration}" по причине "{reason}" (Подробности: {internalDetails})',
        year: 'лет',
        month: 'месяцев',
        week: 'недель',
        day: 'дней',
        codeOfConduct: {
          title: 'Чтобы продолжить, выберите тему',
          chatVerbalAbuse: {
            subject: 'Злоупотребление чатом / оскорбления',
            description:
              'Оскорбление расы, национальности, гендера, пола, сексуальной ориентации и социоэкономического статуса игроков. Пустая болтовня, мелкие обзывательства и ненормативная лексика ни на кого конкретно не направленная, не считаются.',
          },
          abilityAbuse: {
            subject: 'Злоупотребление способностями',
            description:
              'Использование способностей героя, чтобы выдать позицию союзников, поставить их в невыгодное положение или нанести им любой другой вред.',
          },
          itemAbuse: {
            subject: 'Злоупотребление предметами',
            description:
              'Злонамеренная продажа или уничтожение предметов, установка вардов с целью бессмысленно потратить их или заблокировать лагеря нейтралов в собственном лесу, покупка предметов с целью передачи противникам.',
          },
          staffImpersonation: {
            subject: 'Выдача себя за администратора',
            description:
              'Выдача себя за администратора, гейм мастера, модератора, разработчика, хостера, комьюнити менеджера и так далее.',
          },
          threats: {
            subject: 'Угрозы',
            description:
              'Угроза или пожелание вреда, болезни, смерти игрокам, их семьям или друзьям а так же деанон игроков.',
          },
          hateSpeech: {
            subject: 'Разжигание ненависти',
            description:
              'Разжигание ненависти на религиозной, национальной, расовой или любой другой почве.',
          },
          intentToLose: {
            subject: 'Намерение проиграть',
            description:
              'Намеренная отдача героев (включая героев отключённых игроков), саботаж, выдача позиций союзников, отказ от участия в игре (когда игрок последовательно отказывается принимать участие в сражениях и защите построек с намерением слить игру), уклонение от механизма обнаружения AFK, грифинг (когда игрок намеренно не приносит пользы команде в бою, только добивает героев и убегает, или иным образом намеренно раздражает союзников). Выход на чужой лайн, неправильный выбор героя, "неправильное" исполнение роли выбранного героя, слабые навыки игры и отбор фарма у союзников не считаются нарушением, если игрок при этом помогает команде и делает всё, что может, для победы.',
          },
          cheating: {
            subject: 'Читерство',
            description:
              'Передача аккаунта третьим лицам, злоупотребление багами, хаками, эксплойтами, уклонение от бана и тому подобное.',
          },
          inappropriate: {
            subject: 'Неуместное имя / иконка аккаунта',
            description:
              'Нецензурное или оскорбительное имя аккаунта, размещение на иконке аккаунта расистких, сексистских, гомофобных, эротических или отталкивающих изображений (+ удаление иконки аккаунта, если применимо, принудительная смена имени аккаунта, если применимо).',
          },
          statManipulation: {
            subject: 'Манипуляция статистикой',
            description:
              'Намеренный выход из матчей, чтобы уменьшить MMR или увеличить шансы противников на победу. Намеренные попытки манипулировать своим или чужим MMR, KDR, винрейтом и другими статами путём проигрыша матчей или отдач противникам (+ перманентный бан на саб-аккаунт)',
          },
          serialLeaving: {
            subject: 'Многократное покидание матчей',
            description:
              'Намеренный выход из трёх и более матчей в течение суток (включая саб-аккаунты).',
          },
        },
      },
      rapm: {
        title: 'Портал RAP (временное название)',
      },
    },
    staff: {
      promotions: {
        title: 'Назначение и отсранение ГМов',
        promoteToGameMaster: 'Назначить ГМ',
        gmRoster: 'Список ГМов',
        sgmRoster: 'Список старших ГМов',
        adminRoster: 'Список администраторов',
        userName: 'Логин',
        promote: 'Назначить',
        demote: 'Отстранить',
        promoteSuccess: '{userName} успешно назначен ГМ.',
        demoteSuccess: 'ГМ {userName} успешно отстранён.',
      },
      dataPoints: {
        title: 'Работа с базой',
        firstAccountName: 'Первый аккаунт',
        secondAccountName: 'Второй аккаунт',
        lookup: 'Поиск',
        result: 'Результат',
        preDelete: 'Подготовить удаление',
        preDeleteDescription:
          'Нажми на аккаунт, с которого требуется удалить выбранную запись',
        removeSuccessful:
          'Удаление {category}: {dataPoint} с аккаунта {accountName} прошло успешно.',
      },
      suspensions: {
        title: 'Блокировки',
        suspensionID: 'ID блокировки',
        overrideSuccessful: 'Блокировка {suspensionId} успешно изменена.',
        override: 'Изменить блокировку',
        newDurationSuccessful:
          'Длительность блокировки {suspensionId} изменена, новая длительность: {newDuration}. ',
        newDuration: 'Новая длительность',
        changeDuration: 'Изменить длительность блокировки',
        duration: 'Длительность',
        year: 'лет',
        month: 'месяцев',
        week: 'недель',
        day: 'дней',
      },
      enableDisable: {
        title: 'Enable / Disable Account',
        accountName: 'Account name',
        disableAccount: 'Disable account',
        enableAccount: 'Enable account',
        lookup: 'Lookup',
        currentAccountType: 'Current account type: {accountType}',
        successfullyDisabledAccount: 'The account "{accountName}" was disabled',
        successfullyEnabledAccount: 'The account "{accountName}" was enabled',
      },
      promoteToTMMHost: {
        title: 'Promote to TMM host',
        accountNameToPromote: 'Account name',
        promote: 'Pormote',
        successfullyPromotedAccount:
          'The account "{accountName}" was promoted to TMM host',
      },
      currency: {
        title: 'Currency',
        instructions:
          'Use "add row" to add more rows. Empty User Names will be ignored',
        userName: 'User Name',
        gold: 'Gold',
        silver: 'Silver',
        tickets: 'Tickets',
        addRow: 'Add row',
        handleBatchRows: 'Handle batch rows',
        runBatchJob: 'Run batch job',
        result: 'Results',
        batchLogMessage:
          'Added {gold} gold, {silver} silver and {tickets} tickets to {userName}.',
        batchLogErrorMessage: 'Failed to add anything to {userName}.',
      },
      lookUpAccountOrUser: {
        title: 'Look Up Account Or User',
        fuzzyIdentifier: 'Enter Account Name, User Name, Or Email Address',
        lookup: 'Lookup',
        result: 'Result',
        lookupSuspensions: 'Look Up Suspensions',
        enableDisable: 'Enable/Disable',
      },
      actions: {
        title: 'Action history',
        sourceName: 'Source',
        targetName: 'Target',
        action: 'Type of action',
        details: 'Details',
        timestamp: 'Timestamp',
      },
    },
    admin: {
      upgrades: {
        title: 'Upgrades',
        selectUpgrade: 'Select one or more upgrades to add',
        selectUpgradeRemove: 'Select one or more upgrades to remove',
        addUpgrades: 'Add upgrades',
        accountName: 'Account name',
        removeUpgrades: 'Remove upgrades',
        removeSelectedUpgrades: 'Remove "{upgrades}" from "{accountName}"?',
        successfullyAddedUpgrades:
          'Successfully added "{upgrades}" upgrades to "{accountName}"',
        successfullyRemovedUpgrades:
          'Successfully removed "{upgrades}" upgrades from "{accountName}"',
      },
      renameAccount: {
        title: 'Rename account',
        rename: 'Rename',
        accountName: 'Account name',
        newAccountName: 'New account name',
        successfullyRenamedAccount:
          'The account "{accountName}" was renamed to "{newAccountName}"',
      },
    },
  },
}
