using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Prolizy2.Model.Bulletin;

/// <summary>
/// Identity information shown in the student sheet dialog.
/// </summary>
public partial class StudentInfo : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Initials))]
    private string _name = string.Empty;

    [ObservableProperty] private string _birthDate = "—";
    [ObservableProperty] private string _studentId = "—";
    [ObservableProperty] private string _ine = "—";
    [ObservableProperty] private string _program = string.Empty;
    [ObservableProperty] private string _group = string.Empty;

    /// <summary>Two-letter avatar initials, derived from the last two name tokens.</summary>
    public string Initials
    {
        get
        {
            var tokens = (Name ?? string.Empty)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(t => t.Length > 0)
                .ToArray();

            if (tokens.Length == 0)
                return "?";

            var picked = tokens.TakeLast(2).Select(t => char.ToUpperInvariant(t[0]));
            return string.Concat(picked);
        }
    }

    /// <summary>Combined "Programme · Groupe" sub-line.</summary>
    public string ProgramLine =>
        string.IsNullOrWhiteSpace(Group) ? Program : $"{Program} · {Group}";
}
