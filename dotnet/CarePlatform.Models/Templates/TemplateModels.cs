// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
namespace CarePlatform.Models.Templates;

/// <summary>
/// TIU template node for the template tree.
/// </summary>
public class Template
{
    public string Ien { get; set; } = "";
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";          // T=template, F=folder, P=personal, S=shared
    public string BoilerplateText { get; set; } = "";
    public bool IsFolder { get; set; }
    public bool IsPersonal { get; set; }
    public bool HasChildren { get; set; }
    public List<Template> Children { get; set; } = [];
    public List<TemplateField> Fields { get; set; } = [];
}

/// <summary>
/// Template field definition.
/// </summary>
public class TemplateField
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";           // FreeText, Number, Date, Hypertext, Checkbox, RadioButton, Button, ComboBox, EditBox
    public string Default { get; set; } = "";
    public bool Required { get; set; }
    public int MaxLength { get; set; }
    public List<string> Items { get; set; } = [];
    public string Prefix { get; set; } = "";
    public string Suffix { get; set; } = "";
    public bool SeparateLines { get; set; }
}
