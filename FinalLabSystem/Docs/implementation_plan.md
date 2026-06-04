# 🏛 الخطة الهندسية لطبقة العرض (WPF MVVM Architecture & Login Blueprint)

## الهدف (Goal Description)
بناء البنية التحتية الأساسية لنمط MVVM في تطبيق WPF، والبدء بالتنفيذ العمودي (Vertical Slicing) للوحدة الأولى: **شاشة تسجيل الدخول (Login Window)**، لضمان تكاملها بشكل مثالي مع طبقة الخدمات (Service Layer) التي تم الانتهاء منها وتعتمد 100% على الـ Asynchronous Programming.

> [!IMPORTANT]
> **User Review Required:** يرجى مراجعة الحلول المعمارية المقترحة أدناه للتعامل مع `PasswordBox` والتوجيه (Navigation)، والموافقة عليها قبل بدء وكلاء التنفيذ (Execution Agents) في كتابة الكود.

---

## 1. البنية التحتية الأساسية لنمط MVVM (Foundational MVVM Infrastructure)

لتجنب تكرار الكود وضمان استجابة واجهة المستخدم، سيتم بناء الفئات الأساسية التالية في مجلد `Infrastructure` أو `Core`:

### أ. فئة `ViewModelBase`
- **التنفيذ:** ترث من `INotifyPropertyChanged`.
- **المنطق:** توفر دالة `OnPropertyChanged` مدعومة بـ `[CallerMemberName]` لتجنب استخدام أسماء الخصائص كنصوص ثابتة (Hardcoded strings).
- **الهدف:** توفير آلية التحديث التلقائي للواجهة (Data Binding) بشكل نظيف وسريع لكل الـ ViewModels المستقبلية.

### ب. فئة `RelayCommand` و `AsyncRelayCommand<T>`
- **`RelayCommand`:** للعمليات البسيطة المتزامنة (مثل تبديل رؤية كلمة المرور).
- **`AsyncRelayCommand` / `AsyncRelayCommand<T>`:** لتغليف الـ `Task` وضمان عدم تجميد الـ UI Thread أثناء انتظار طبقة الخدمات (مثل `await _authService.LoginAsync`). 
- **المعالجة:** يجب أن يحتوي `AsyncRelayCommand` على `try/catch` داخلي أو يمرر الخطأ للدالة المستدعية لمنع انهيار التطبيق، مع القدرة على تعطيل الزر (CanExecute = false) أثناء المعالجة (IsExecuting).

---

## 2. مواصفات الوحدة الأولى: شاشة تسجيل الدخول (Module 1: Login)

### أ. الـ ViewModel (`LoginViewModel`)
سيتم إنشاؤه في مجلد `ViewModels`.
- **الحقن (DI):** `IAuthService` عبر الـ Constructor.
- **الخصائص المرتبطة (Bound Properties):**
  - `Username` (string)
  - `Password` (string) - *سيتم ربطه باستخدام Attached Property.*
  - `IsRememberMe` (bool)
  - `IsPasswordVisible` (bool)
  - `ErrorMessage` (string) - يعرض رسالة الخطأ للمستخدم في حال فشل تسجيل الدخول.
  - `IsBusy` (bool) - لإظهار Loading Spinner أو تعطيل الزر أثناء الاتصال.
- **المنطق الأولي (Initialization):** قراءة الإعدادات المحفوظة (مثل `Properties.Settings.Default` أو ملف JSON محلي) لتعبئة `Username` إذا كان الحفظ مفعلاً مسبقاً.
- **أمر تسجيل الدخول (`LoginCommand`):**
  - استخدام `AsyncRelayCommand`.
  - الخطوات: `IsBusy = true` -> تفريغ `ErrorMessage` -> استدعاء `_authService.LoginAsync` -> إذا نجح، الانتقال للنافذة الرئيسية (`MainWindow`) -> إذا فشل، تعيين `ErrorMessage` -> `IsBusy = false`.

### ب. الـ View (`LoginView.xaml`)
سيتم إنشاؤه في مجلد `Views`. الواجهة ستكون جذابة ومبنية بأفضل ممارسات WPF.
- **التصميم:** استخدام `Grid` نظيف. الحقول ستكون `TextBox` لاسم المستخدم، و `PasswordBox` لكلمة المرور.

> [!WARNING]
> **تحدي ربط `PasswordBox` (The Bindability Challenge):**
> الـ `PasswordBox` في WPF لا يدعم `Binding` لخاصية `Password` لأسباب أمنية. لتجنب انتهاك نمط MVVM وكتابة كود في Code-Behind:
> **الحل المعماري (Architectural Solution):** سيتم إنشاء فئة مساعدة `PasswordBoxHelper` تحتوي على `Attached Dependency Property` تقوم بالاستماع لتغيرات الـ `PasswordBox` وتحديث خاصية `Password` في الـ `LoginViewModel`، والعكس. 

- **زر إظهار/إخفاء كلمة المرور (Eye Icon):**
  - سيتم ربطه بـ `IsPasswordVisible` في الـ ViewModel عبر زر.
  - سيتم استخدام `DataTrigger` في ملف XAML: 
    - عندما يكون `IsPasswordVisible = true`، يظهر `TextBox` عادي يعرض النص المرتبط بالـ `Password` ويختفي الـ `PasswordBox`.
    - عندما يكون `False`، يعود الـ `PasswordBox` للظهور ويختفي الـ `TextBox`.

---

## 3. أسئلة مفتوحة ومقترحات للمراجعة (Open Questions & Proposals)
1. **آلية التوجيه (Navigation):** هل تفضل استخدام آلية انتقال مبنية على النوافذ (Window-based) (إخفاء نافذة الدخول وإظهار `MainWindow`)، أم آلية (Page/UserControl-based) داخل نافذة تطبيق رئيسية واحدة للتبديل بين الـ Views؟
2. **الحفظ الآمن (Remember Me):** أين ترغب بحفظ اسم المستخدم بشكل محلي؟ هل نستخدم `Properties.Settings` القياسية لـ .NET أم ننشئ ملف تكوين مخصص (JSON Configuration File)؟

---

## 4. خطة التحقق (Verification Plan)
### الاختبارات الآلية / التنفيذ المستقل
- تشغيل التطبيق والتأكد من فتح شاشة تسجيل الدخول بأسلوب معماري صحيح (ربط `DataContext` بشكل نظيف، إما عبر Code-Behind بسيط للحقن أو عبر DI Container مثل `Microsoft.Extensions.DependencyInjection`).
- التأكد من عمل `PasswordBoxHelper` بسلاسة دون تعارضات.

### الاختبار اليدوي
- إدخال بيانات صحيحة ومراقبة استجابة الـ ViewModel واستدعاء `IAuthService` بشكل غیر متزامن (`Async`).
- التحقق من تفاعل الواجهة (الزر يصبح معطلاً والـ Spinner يعمل) أثناء الانتظار.
- تجربة زر "العين" لإخفاء وإظهار كلمة المرور.
