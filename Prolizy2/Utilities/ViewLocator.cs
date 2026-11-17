using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace Prolizy2.Utilities;

public class ViewLocator : IDataTemplate
{
    // Weak association: a view lives as long as its view model, instead of every
    // dialog ever opened being kept alive by a strong dictionary.
    private readonly ConditionalWeakTable<object, Control> _controlCache = new();

    public Control Build(object? data)
    {
        if(data is null) 
            return new TextBlock { Text = "Data is null." };
        
        var fullName = data.GetType().FullName;
        
        if (string.IsNullOrWhiteSpace(fullName)) 
            return new TextBlock { Text = "Type has no name, or name is empty." };
        
        // Convert ViewModel namespace and class name to corresponding View
        // E.g.: Prolizy2.ViewModels.Dialogs.GroupSelectionDialogViewModel
        //   ->  Prolizy2.Views.Dialogs.GroupSelectionDialog
        var name = fullName.Replace(".ViewModels.", ".Views.").Replace("ViewModel", "");
        var type = Type.GetType(name);
        
        if (type is null)
        {
            Logs.Warn("No view found for '" + fullName + "' -> '" + name + "'");
            return new TextBlock { Text = $"No View For {name}." };
        }

        if (!_controlCache.TryGetValue(data, out var res))
        {
            res = (Control)Activator.CreateInstance(type)!;
            _controlCache.Add(data, res);
        }

        res.DataContext = data;
        return res;
    }

    public bool Match(object? data) => data is INotifyPropertyChanged;
}