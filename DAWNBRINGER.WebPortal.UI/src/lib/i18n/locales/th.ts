export default {
  apiError: {
    // Unhandled Error Page
    title: 'มีบางอย่างผิดพลาด!',
    ok: 'ตกลง',
    serviceUnavailable: 'ไม่สามารถใช้บริการได้ในขณะนี้',
  },

  validation: {
    title: 'บางอย่างผิดพลาด',
    rules: {
      required: 'กรุณากรอก {key}',
      email: 'ช่อง {key} ต้องเป็นที่อยู่อีเมล',
      match: 'ช่อง {key} และ {param} ต้องตรงกัน',
      passwordPolicy:
        'ช่อง {key} ต้องมีตัวอักษรทั้งหมดอย่างน้อย 8 ตัว, อักษรอังกฤษตัวพิมพ์เล็กและใหญ่อย่างน้อยหนึ่งตัวเช่น Aa ,ตัวเลขและตัวอักษรพิเศษอย่างน้อยหนึ่งตัวเช่น !@#',
      usernamePolicy:
        'ช่อง {key} ต้องประกอบด้วย A-Z 0-9 และ ` _ - เท่านั้น และมีความยาวระหว่าง 4 ถึง 12 ตัว',
    },
  },

  fp: {
    title: 'หน้าหลัก',
    ladder: 'อันดับผู้เล่น',
    community: 'เข้าร่วมเซิร์ฟเวอร์ Discord',
    login: 'เข้าสู่ระบบ',
    playNow: 'เริ่มเล่นตอนนี้',
    playWith: 'ร่วมเล่น HoN กับผู้เล่นจำนวน {amount} คน ตอนนี้!',
    register: 'ลงทะเบียน',
    playersOnline: 'ผู้เล่นออนไลน์ในขณะนี้',
    registeredAccounts: 'บัญชีที่ลงทะเบียน',
    matchesInProgress: 'แมทช์ที่กำลังดำเนินอยู่',
    featuredStreamer: 'สตรีมเมอร์ยอดนิยม',
    showFestivities: 'เปิดใช้งานธีมตามฤดูกาล',
    timeline: {
      title: 'ประวัติ Project KONGOR',
      a: {
        date: '13 ธันวาคม 2564',
        title: 'ประกาศวันที่ยุติการให้บริการของ HoN',
        text: 'ทีมพัฒนา Frostburn Studio เริ่มประกาศว่าจะปิดตัวลงอย่างเป็นทางการในวันที่ 20 มิถุนายน 2565',
      },
      b: {
        date: '13 ธันวาคม 2564',
        title: 'เริ่มต้นพัฒนา Project KONGOR ',
        text: 'เริ่มต้นเปิดเซิร์ฟเวอร์หลักเพื่อทดสอบประกอบด้วยการลองเข้าสู่ระบบใช้งานฟังก์ชั่นร้านค้าและปิงโกะเป็นครั้งแรก',
      },
      c: {
        date: '20 มิถุนายน 2565',
        title: 'เซิร์ฟเวอร์ HoN ปิดตัวลงอย่างเป็นทางการ',
        text: 'เกมเปิดตัวครั้งแรกเมื่อวันที่ 12 พฤษภาคม 2553 เป็นเวลา 13 ปีก่อนที่จะหยุดให้บริการ',
      },
      d: {
        date: '5 สิงหาคม 2565',
        title: 'Project KONGOR เข้าสู่ช่วงทดสอบ Open Beta',
        text: 'Project KONGOR เปิดให้ผู้เล่นทุกคนสามารถเข้าร่วมทดสอบใช้งานระบบแบบสาธารณะ',
      },
      e: {
        date: '5 มกราคม 2566',
        title: 'Project KONGOR ได้สำเร็จเป้าหมายสำคัญ',
        text: 'มีผู้เล่นออนไลน์จำนวน 4,700 คน และเข้าร่วมเล่นทั้งหมด 1,000,000 เกม ตั้งแต่เดือนสิงหาคม 2565',
      },
    },
  },

  ladder: {
    title: 'อันดับ',
    gameModes: {
      caldavar: 'Caldavar',
      caldavarCasual: 'Caldavar (Casual)',
      midwars: 'Midwars',
      warEffort: 'The War Effort (เควสต์)',
    },
    subTitle: 'ท็อป {x} ผู้เล่นในโหมด {gameMode}',
    search: 'ค้นหา',
    mmr: '{rating} MMR',
    gamesPlayed: 'เกมที่เล่น',
    winRate: 'อัตราชนะ',
    KDA: 'K/D/A',
    GPM: 'GPM',
    APM: 'APM',
  },

  login: {
    // Login Page
    title: 'เข้าสู่ระบบ',
    username: 'ชื่อผู้ใช้งาน',
    password: 'รหัสผ่าน',
    passwordMatch: 'ยืนยันรหัสผ่าน',
    forgotPassword: 'ลืมรหัสผ่าน',
    login: 'เข้าสู่ระบบ',
    youMustBeAuthed: 'จำเป็นต้องเข้าสู่ระบบเพื่อดูหน้านี้',
    youAreLoggedOut: 'ออกจากระบบแล้ว',
    emailAddress: 'ที่อยู่อีเมล',
    forgotPasswordSubmit: 'ส่งอีเมลรีเซ็ตรหัสผ่าน',
    changePasswordAction: 'อัปเดตรหัสผ่าน',
    passwordRequestResetSuccessTitle: 'ส่งคำขออัปเดตรหัสผ่านแล้ว',
    passwordRequestResetSuccessMessage:
      'ตรวจสอบอีเมลที่ส่งไปยัง "{email}" สำหรับรายละเอียดการอัปเดตรหัสผ่าน',
    changePasswordTitle: 'อัปเดตรหัสผ่านของคุณ',
    changePasswordSuccessTitle: 'รหัสผ่านของคุณได้รับการอัปเดต',
    changePasswordSuccessMessage: 'รหัสผ่านของคุณได้รับการอัปเดตเรียบร้อยแล้ว!',
  },

  register: {
    // Registration Page
    title: 'ลงทะเบียน',
    username: 'ชื่อผู้ใช้งาน',
    password: 'รหัสผ่าน',
    passwordMatch: 'ยืนยันรหัสผ่าน',
    emailAddress: 'ที่อยู่อีเมล',
    emailAddressMatch: 'ยืนยันที่อยู่อีเมล',
    registerAction: 'ลงทะเบียน',
    registerSuccessTitle: 'การลงทะเบียนอีเมลสำเร็จ',
    registerSuccessfulMessage:
      'ตรวจสอบอีเมลที่ส่งไปยัง "{email}" สำหรับรายละเอียดการลงทะเบียน',
    completeRegisterAction: 'การลงทะเบียนเสร็จสิ้น',
    completeYourRegistrationTitle: 'การลงทะเบียนเสร็จสิ้น',
    registerCompleteSuccessTitle: 'บัญชีของคุณลงทะเบียนสำเร็จแล้ว',
    registerCompleteSuccessMessage:
      'หากคุณยังไม่ได้ติดตั้งตัวเกม HoN และต้องการความช่วยเหลือ สามารถเข้ามาในชุมชนดิสคอร์ดเพื่อถามสอบถามรายละเอียดต่างๆ',
    joinOurDiscord: 'เข้าร่วมเซิร์ฟเวอร์ Discord ของเรา',
  },

  dashboard: {
    // Account Page
    title: 'บัญชี',
    mySettingsBtn: 'ตั้งค่าบัญชี',
    logout: 'ออกจากระบบ',
    welcome: 'ผู้เล่น, {name}',
    menuTitles: {
      gmTools: 'Game Master Tools',
      sgmTools: 'Senior Game Master Tools',
      adminTools: 'Administrator tools',
    },
    youAreNotAllowedHere:
      'คุณไม่สามารถเห็นหน้านี้ ขออภัย หากคุณเชื่อว่าสิ่งนี้ไม่ถูกต้องโปรดสอบถามผู้ดูแลระบบ',

    mySettings: {
      // Account Settings Sub-Page
      title: 'การตั้งค่าบัญชี',

      changeEmail: {
        // Update Email Tab
        title: 'อัปเดตที่อยู่อีเมล',
        emailNotVerified: 'ไม่สามารถยืนยันที่อยู่อีเมล',
        emailVerified: 'ยืนยันที่อยู่อีเมลแล้ว',
        emailAddress: 'ที่อยู่อีเมลใหม่',
        emailAddressMatch: 'ยืนยันที่อยู่อีเมลใหม่',
        changeEmailAction: 'อัปเดตที่อยู่อีเมล',
        password: 'รหัสผ่านปัจจุบัน',
        changeRequestSuccessTitle: 'ส่งคำขออัปเดตที่อยู่อีเมลแล้ว',
        changeRequestSuccessMessage:
          'ตรวจสอบอีเมลที่ส่งไปยัง "{email}" สำหรับรายละเอียดการอัปเดตที่อยู่อีเมล',
        emailChangedSuccessfully:
          'ที่อยู่อีเมลของคุณได้รับการอัปเดตเรียบร้อยแล้ว',
      },
    },

    cai: {
      title: 'อัปโหลดรูปไอคอน',
      selectFile: 'Click here to select a file and continue.',
      instructions:
        'ใช้ลูกกลิ้งเมาส์เพื่อซูมเข้าและออก คลิกเมาส์ค้างไว้เพื่อเลื่อนตำแหน่งรูปภาพ',
      uploadAction: 'อัปโหลดรูปภาพ',
      uploadedWithSuccess:
        'เพิ่มไอคอนบัญชีที่กำหนดเองแล้ว คุณจำเป็นต้องเริ่มเกมใหม่ก่อนที่จะเห็นในคลัง!',
    },

    gm: {
      suspension: {
        title: 'ระงับผู้เล่น',
        lookup: 'ค้นหาข้อมูลบัญชี',
        suspensionId: 'Suspension #{id}',
        previousSuspensions: 'ประวัติการถูกระงับ ({count}) ครั้ง',
        noBanMessage: 'บัญชี "{accountName}" ไม่มีเคยถูกระงับ',
        accountName: 'ชื่อบัญชี',
        duration: 'ระยะเวลา',
        reason: 'เหตุผล',
        internalDetails:
          'รายละเอียดภายใน (มองเห็นได้เฉพาะ GM และผู้ดูแลระบบเท่านั้น)',
        submit: 'ส่ง',
        suspensionSuccessful:
          'ระงับสำเร็จ บัญชี "{accountName}" ได้รับโทษเป็นระยะเวลา "{duration}" โดยเหตุผล "{reason}"',
        year: 'ปี',
        month: 'เดือน',
        week: 'สัปดาห์',
        day: 'วัน',
        codeOfConduct: {
          title: 'เลือกหัวข้อเพื่อดำเนินการต่อ',
          chatVerbalAbuse: {
            subject: 'ใช้ข้อความที่ไม่สมควร/แชท',
            description:
              'การใช้ถ้อยคำดูหมิ่นคุกคาม เหยียดหยาม เชื้อชาติ เพศ และการล่วงละเมิดต่างๆ(การพูดจาไร้สาระ การพูดคำหยาบ การเรียกคนอื่นว่าอ่อน เป็นเรื่องปกติ',
          },
          abilityAbuse: {
            subject: 'ใช้ความสามารถในทางที่ผิด',
            description:
              'การใช้สกิลหรือความสามารถฮีโร่ในทางที่ผิด เจตนาในทางที่ไม่ดี หรือ ส่งผลร้ายต่อผู้อื่น เช่น โคนอส เจตนาโดมเพื่อนให้ศัตรูฆ่า เพบเบิล เจตนาโยนเพื่อนไปให้ศัตรูฆ่า และอื่นๆ',
          },
          itemAbuse: {
            subject: 'ใช้ไอเทมในทางที่ผิด',
            description:
              'ตั้งใจขายหรือทำลาย ไอเท็ม โดยไม่มีเหตุผล หรือการ ปักวอร์ดที่ส่งผลกระทบกับทีม เช่น ปักวอร์ดบ่อน้ำ วอร์ดครีปทีมตัวเอง หรือการ ซื้อไอเทมและตั้งใจดรอปให้ทีมศัครู',
          },
          staffImpersonation: {
            subject: 'แอบอ้างเป็นทีมงาน',
            description:
              'ผู้เล่นที่แอบอ้างตัวเองว่าเป็นทีมงาน (Game Master, Developer, Server Host, Community Manager, Moderator, Collaborator และอื่นๆ) หรือลักษณะที่ทำให้คนอื่นเชื่อว่าตนเองเป็นผู้เกี่ยวข้องกับทีมงาน',
          },
          threats: {
            subject: 'คุกคาม',
            description:
              'ผู้เล่นที่มีเจตนาไม่ดีต่อผู้เล่นคนอื่น เช่น ขู่ว่าจะทำร้ายหรือการขู่เอาชีวิตไม่ว่าจะเป็นเพื่อนหรือครอบครัว และอื่นๆ',
          },
          hateSpeech: {
            subject: 'ใช้คำพูดที่ไม่สมควร/เสียง',
            description:
              'การใช้ถ้อยคำดูหมิ่นคุกคาม เหยีดหยาม เชื้อชาติ เพศ และการล่วงละเมิดต่างๆ',
          },
          intentToLose: {
            subject: 'เจตนาโยนเกม',
            description:
              'การตายโดยเจตนา การระบุตำแหน่งเพื่อนร่วมทีม การปฏิเสธที่จะให้ความมือกับทีม (เช่น ไม่ป้องกันป้อมและเมื่อทีมขอให้ดันป้อม ฯลฯ) เจตนายืน AFK เพื่อโยนเกม เจตนา Last Kills เพื่อนร่วมทีม การกระทำหรือพฤติกรรมที่ส่งผลเสียต่อผลการแข่งขัน ฯลฯ (การแย่งเลน การเลือกฮีโร่ที่ไม่ดี การเล่นเกมที่ไม่ดี หรือแย่งฟาร์มไม่สามารถแบนได้ เว้นแต่คุณจะมีเหตุผลให้เชื่อว่าพวกเขามีเจตนาไม่ดี)',
          },
          cheating: {
            subject: 'โกง/ใช้บัค',
            description:
              'ผู้เล่นที่ใช้ช่องโหว่ บัค/แฮ๊ก เพื่อหาผลประโยขน์ให้ตัวเอง และการใช้โปรแกรมภายนอกที่หาผลประโยขน์ให้ตัวเองได้เปรียบ เช่น ACD',
          },
          inappropriate: {
            subject: 'ใช้ชื่อและรูปภาพไอคอนที่ไม่เหมาะสม',
            description:
              'ผู้เล่นที่ใช้ชื่อบัญชีและ ไอคอน ไปในทางที่ไม่สมควร เช่น เหยียดเชื้อชาติ เหยียดเพศ หยาบคาย น่ารังเกียจ หรือมีภาพโป๊เปลือย ฯลฯ',
          },
          statManipulation: {
            subject: 'ปั้มสถิติ/ลดแรงค์',
            description:
              'จงใจตัดการเชื่อมต่อจากเกมเพื่อลด MMR เพิ่ม MMR ของตัวผู้เล่นเองหรือทีมฝ่ายตรงข้าม ตั้งใจแพ้/ชนะ/ตายให้กับผู้เล่นฝายตรงข้ามเพื่อเพิ่มหรือลดอัตราการชะ/MMR/KDR เป็นต้น (ส่งผลทำให้ระงับบัญชีย่อยถาวร)',
          },
          serialLeaving: {
            subject: 'เจตนาตัดการเชื่อมต่อจากเกม',
            description:
              'หากผู้เล่นออก 3 เกมขึ้นไปภายใน 1 วัน (รวมถึงบัญชีย่อย)',
          },
        },
      },
      rapm: {
        title: 'จัดการรายงานผู้เล่น',
      },
    },
    staff: {
      promotions: {
        title: 'เลื่อนระดับ/ลดระดับ Game Master',
        promoteToGameMaster: 'เลื่อนระดับเป็น Game Master',
        gmRoster: 'รายชื่อ Game Master',
        sgmRoster: 'รายชื่อ Senior Game Master',
        adminRoster: 'รายชื่อ Administrator',
        userName: 'ชื่อบัญชี',
        promote: 'เลื่อนระดับ',
        demote: 'ลดระดับ',
        promoteSuccess:
          'ประสบความสำเร็จในการเลื่อนระดับ {userName} เป็น Game Master',
        demoteSuccess: 'ลดระดับ {userName} ในการเป็น Game Master',
      },
      dataPoints: {
        title: 'ข้อมูลผู้เล่น',
        firstAccountName: 'ชื่อบัญชีแรก',
        secondAccountName: 'ชื่อบัญชีที่สอง',
        lookup: 'ค้นหา',
        result: 'ผลลัพธ์',
        preDelete: 'ลบข้อมูล',
        preDeleteDescription: 'โปรดคลิกชื่อบัญชีที่คุณต้องการลบข้อมูลที่เลือก',
        removeSuccessful:
          'ดำเนินการลบ {category} "{dataPoint}" จาก {accountName} เรียบร้อยแล้ว',
      },
      suspensions: {
        title: 'การระงับ',
        suspensionID: 'รหัสการระงับ',
        overrideSuccessful: 'แทนที่การระงับ {suspensionId} สำเร็จ',
        override: 'แทนที่การระงับ',
        newDurationSuccessful:
          'การระงับ {suspensionId} ระยะเวลาเปลี่ยนเป็น {newDuration} สำเร็จ',
        newDuration: 'ระยะเวลาใหม่',
        changeDuration: 'เปลี่ยนระยะเวลาการระงับ',
        duration: 'ระยะเวลา',
        year: 'ปี',
        month: 'เดือน',
        week: 'สัปดาห์',
        day: 'วัน',
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
  quests: {
    title: 'The War Effort (เควสต์และภารกิจ)',
    subtitle: 'ทำภารกิจรายวันและสะสมคะแนนเกียรติยศ (Reputation) เพื่อปลดล็อก 14 หีบรางวัลในรอบ 28 วัน',
    warEffortCycle: 'The War Effort — รอบที่ {cycle}',
    cycleEndsIn: 'รอบสงครามสิ้นสุดในอีก',
    totalReputation: 'คะแนนเกียรติยศสะสมรอบนี้',
    activeTier: 'เป้าหมายถัดไป',
    tierProgress: 'ความคืบหน้าหีบปัจจุบัน',
    openBag: 'เปิดรับรางวัล',
    opened: 'เปิดแล้ว',
    locked: 'ยังไม่ปลดล็อก',
    readyToOpen: 'ปลดล็อกแล้ว (กดรับรางวัล)',
    satchel: 'ถุงเสบียง',
    chest: 'หีบสมบัติ',
    finalChest: 'Treasure of Champions',
    dailySlots: 'ภารกิจประจำวัน (Daily Quests)',
    dailyTitle: 'ภารกิจรายวัน (Daily Quests)',
    weeklyTitle: 'ภารกิจประจำสัปดาห์',
    progress: 'ความคืบหน้า',
    completed: 'ภารกิจสำเร็จแล้ว',
    claimed: 'รับรางวัลแล้ว',
    claimReward: 'รับของรางวัล',
    resetsIn: 'รีเซ็ตในอีก',
    rewards: 'ของรางวัล',
    reputationReward: '+{points} คะแนนเกียรติยศ (RP)',
    bagOpenedTitle: 'ยินดีด้วย! คุณได้รับของรางวัล',
    simulateMatch: 'จำลองผลแมตช์ (สำหรับทดสอบเควสต์ & War Effort)',
    simulateWin: '+1 ชนะแมตช์ (Caldavar/Mid Wars)',
    simulateKill: '+1 สังหารฮีโร่ (Hero Kill)',
    simulateWard: '+1 ปักหวาด (Ward Placed)',
    simulateAssist: '+1 ช่วยเหลือ (Assist)',
    simulateMatchPlayed: '+1 เล่นจบแมตช์',
  },
  minigames: {
    title: 'วงล้อเสี่ยงโชค',
    subtitle: 'Heroes of Newerth Lucky Wheel of Fortune (หมุนฟรีทุก 8 ชั่วโมง)',
    checkInTitle: 'เช็กอินต่อเนื่อง 7 วัน',
    checkInSubtitle: 'ล็อกอินทุกวันเพื่อรับเหรียญและตั๋วเสี่ยงโชคสะสม',
    checkInButton: 'เช็กอินวันนี้',
    alreadyCheckedIn: 'เช็กอินวันนี้แล้ว',
    dayStreak: 'วันที่ {day}',
    jackpotDay: 'วันแจ็กพอตใหญ่!',
    luckySpinTitle: 'วงล้อเสี่ยงโชค (Lucky Spin)',
    luckySpinSubtitle: 'หมุนวงล้อฟรีได้ทุกๆ 8 ชั่วโมง หรือใช้ตั๋ว Plinko หมุนเพิ่ม',
    spinFree: 'หมุนฟรี (ทุก 8 ชม.)',
    spinWithTicket: 'หมุนด้วยตั๋ว (ใช้ 1 ใบ)',
    freeSpinsLeft: 'มีสิทธิ์หมุนฟรีแล้ว!',
    ticketBalance: 'ตั๋วเสี่ยงโชค',
    spinning: 'กำลังหมุน...',
    congratulations: 'ยินดีด้วย!',
    totalSpins: 'จำนวนครั้งที่หมุนทั้งหมด',
    nextFreeSpinIn: 'หมุนฟรีครั้งถัดไปในอีก',
    freeSpinReady: 'พร้อมหมุนฟรีแล้ว!',
    cooldownWaiting: 'รอหมุนฟรีในอีก',
  },
}
