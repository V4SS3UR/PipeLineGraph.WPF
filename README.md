# ToastManager

ToastManager is a simple C# library for displaying customizable toast notifications or views as overlay in WPF (Windows Presentation Foundation) applications.

## Features

- **Toast Types:** Display toast messages categorized as Warning, Error, Success, and Info.
- **Customization:** Customize toast messages with different titles, messages, icons, and button configurations.
- **MVVM Support:** Designed with MVVM architecture for seamless integration into WPF projects.
- **Responsive Design:** Toasts are responsive and can adapt to any different style or template.
- **Flexibility:** Display custom controls or views instead of traditional toast notifications for richer user experiences.
  
## Installation

Include it manually in your project.

### Usage

#### Display Toast Messages

```cs
// Show a warning toast
ToastManager.ShowWarningToast("This is a warning message");

// Show an error toast
ToastManager.ShowErrorToast("An error occurred");

// Show a success toast
ToastManager.ShowSuccessToast("Operation successful");

// Show an info toast
ToastManager.ShowInfoToast("Information message");

// Show a custom toast
ToastManager.ShowToast("Custom message", "Custom Title", ToastType.Info, ToastButton.Yes | ToastButton.No);
```

#### Display custom view
```cs
// Bring a custom control as toast
var toastView = new MyView();
ToastManager.ShowToast(toastView);
```

#### Handle Toast Actions

You can handle actions like button clicks or toast dismissal in your ViewModel:

```cs
var toastView = new Toast_View(message, title, toastType, toastButton);
toastView.OnOkButtonClick += () => foo();
toastView.OnYesButtonClick += () => foo();
toastView.OnNoButtonClick += () => foo();
toastView.OnCancelButtonClick += () => foo();
toastView.OnExitButtonClick += () => foo();
```

### Customize Toast Views

Customize the appearance and behavior of toast views by modifying the template of the [Toast_View.xaml](https://github.com/V4SS3UR/ToastManager.WPF/blob/main/ToastManager/WPF/View/Toast_View.xaml) UserControl provided in the library. 

## Contributing

Contributions are welcome! If you'd like to contribute to ToastManager, please follow these steps:

## License

This project is licensed under the MIT License - see the LICENSE file for details.


---

Inspired by the need for simple and dynamic toast notifications in WPF applications. Elevate your user interface with ToastManager !
