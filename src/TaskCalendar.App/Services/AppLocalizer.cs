namespace TaskCalendar.App.Services;

public sealed class AppLocalizer
{
    private readonly SessionService _sessionService;

    private readonly Dictionary<string, (string Es, string En)> _texts = new()
    {
        ["app_name"] = ("Task Calendar", "Task Calendar"),
        ["welcome"] = ("Organiza tus bloques de trabajo con reglas reales.", "Organize your work blocks with real scheduling rules."),
        ["login"] = ("Ingresar", "Login"),
        ["register"] = ("Crear cuenta", "Register"),
        ["logout"] = ("Salir", "Logout"),
        ["calendar"] = ("Calendario", "Calendar"),
        ["settings"] = ("Configuracion", "Settings"),
        ["tasks"] = ("Tareas", "Tasks"),
        ["daily"] = ("Diario", "Daily"),
        ["weekly"] = ("Semanal", "Weekly"),
        ["monthly"] = ("Mensual", "Monthly"),
        ["today"] = ("Hoy", "Today"),
        ["save"] = ("Guardar", "Save"),
        ["delete"] = ("Eliminar", "Delete"),
        ["edit"] = ("Editar", "Edit"),
        ["refresh"] = ("Actualizar", "Refresh"),
        ["operating_hours"] = ("Horario operativo", "Operating hours"),
        ["title"] = ("Titulo", "Title"),
        ["description"] = ("Descripcion", "Description"),
        ["priority"] = ("Prioridad", "Priority"),
        ["status"] = ("Estado", "Status"),
        ["start"] = ("Inicio", "Start"),
        ["end"] = ("Fin", "End"),
        ["repeat"] = ("Repeticion", "Recurrence"),
        ["repeat_until"] = ("Repetir hasta", "Repeat until"),
        ["view_range"] = ("Rango visible", "Visible range"),
        ["new_task"] = ("Nueva tarea", "New task"),
        ["edit_task"] = ("Editar tarea", "Edit task"),
        ["email"] = ("Correo", "Email"),
        ["password"] = ("Contrasena", "Password"),
        ["display_name"] = ("Nombre visible", "Display name"),
        ["language"] = ("Idioma", "Language"),
        ["seed_hint"] = ("Usuario demo: demo@taskcalendar.local / Demo123!", "Demo user: demo@taskcalendar.local / Demo123!"),
        ["definitions"] = ("Series configuradas", "Configured series"),
        ["occurrences"] = ("Bloques en el calendario", "Calendar blocks"),
        ["enabled"] = ("Disponible", "Enabled"),
        ["actions"] = ("Acciones", "Actions"),
        ["no_data"] = ("No hay datos para mostrar.", "No data to display."),
        ["authenticated_as"] = ("Sesion iniciada como", "Signed in as"),
        ["go_calendar"] = ("Ir al calendario", "Open calendar")
    };

    public AppLocalizer(SessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public string this[string key]
    {
        get
        {
            var language = _sessionService.Language;
            if (!_texts.TryGetValue(key, out var text))
            {
                return key;
            }

            return language == "en" ? text.En : text.Es;
        }
    }
}
