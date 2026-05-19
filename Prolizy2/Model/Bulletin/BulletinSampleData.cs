using System.Collections.ObjectModel;

namespace Prolizy2.Model.Bulletin;

/// <summary>
/// Builds a representative stub bulletin (Semestre 2 — BUT Informatique),
/// transcribed from the design hand-off sample data. Used for design-time
/// previews and until the hosting application wires real ScoDoc data in.
/// </summary>
public static class BulletinSampleData
{
    public static BulletinData Create()
    {
        var data = new BulletinData
        {
            Student = new StudentInfo
            {
                Name = "M. RACOT NICOLAS",
                BirthDate = "29/09/2006",
                StudentId = "22400720",
                Ine = "—",
                Program = "BUT Informatique",
                Group = "A - B"
            },
            Rank = 11,
            Total = 71,
            Decision = "ADM",
            DecisionLabel = "Admis",
            FinePrint =
                "Décision année : ADM — Admis. Inscrit le 30/01/2025. " +
                "UE acquises : UE2.1 → UE2.6. Autorisé à s'inscrire en S3.\n" +
                "Les moyennes servent à situer l'étudiant dans la promotion et ne " +
                "correspondent pas à des validations de compétences ou d'UE."
        };

        data.Semesters = new ObservableCollection<SemesterInfo>
        {
            new() { Id = "s4", Code = "S4", Year = "2025/2026", Track = "FA", IsFuture = true },
            new() { Id = "s3", Code = "S3", Year = "2025/2026", Track = "FA", IsFuture = true },
            new()
            {
                Id = "s2", Code = "S2", Year = "2024/2025", Track = "FI", IsCurrent = true,
                Decision = "ADM", DecisionLabel = "Admis", EnrollmentDate = "30/01/2025"
            },
            new()
            {
                Id = "s1", Code = "S1", Year = "2024/2025", Track = "FI",
                Decision = "ADM", DecisionLabel = "Admis"
            }
        };

        data.Units = new ObservableCollection<TeachingUnitInfo>
        {
            Unit("UE2.1", "#2563EB", "Développer des applications informatiques simples", "Réaliser",
                17.90, 3, 71,
                Contribution("R2.01", "Dev. objets", 19.13, 21),
                Contribution("R2.02", "Dev. d'apps avec IHM", 19.50, 21),
                Contribution("R2.03", "Qualité de dev.", 13.88, 12),
                Contribution("R2.13", "Comm. prof.", 14.29, 6),
                Contribution("SAE2.01", "Développement d'une application", 18.25, 38),
                Contribution("P2.01", "Portfolio", 16.33, 2)),
            Unit("UE2.2", "#7C3AED", "Appréhender et construire des algorithmes", "Optimiser",
                13.53, 16, 71,
                Contribution("R2.01", "Dev. objets", 19.13, 15),
                Contribution("R2.04", "Réseau & bas niveau", 10.00, 12),
                Contribution("R2.07", "Graphes", 10.00, 21),
                Contribution("R2.09", "Méthodes numériques", 9.25, 12),
                Contribution("SAE2.02", "Exploration algorithmique d'un problème", 15.60, 38),
                Contribution("P2.01", "Portfolio", 16.33, 2)),
            Unit("UE2.3", "#0D9488", "Installer et configurer un poste de travail", "Administrer",
                12.34, 2, 71,
                Contribution("R2.04", "Réseau & bas niveau", 10.00, 36),
                Contribution("R2.05", "Services réseau", 12.00, 15),
                Contribution("R2.12", "Anglais entreprise", 14.43, 6),
                Contribution("R2.13", "Comm. prof.", 14.29, 3),
                Contribution("SAE2.03", "Installation de services réseau", 14.00, 38),
                Contribution("P2.01", "Portfolio", 16.33, 2)),
            Unit("UE2.4", "#D97706",
                "Concevoir et mettre en place une base de données à partir d'un cahier des charges client",
                "Gérer", 11.31, 9, 72,
                Contribution("R2.06", "Exploitation BD", 11.25, 30),
                Contribution("R2.08", "Stats descriptives", 4.00, 12),
                Contribution("R2.10", "Gestion proj. orga.", 8.23, 12),
                Contribution("R2.12", "Anglais entreprise", 14.43, 6),
                Contribution("SAE2.04", "Exploitation BD", 13.88, 38),
                Contribution("P2.01", "Portfolio", 16.33, 2)),
            Unit("UE2.5", "#DC2626", "Identifier les besoins métiers des clients et des utilisateurs",
                "Conduire", 11.81, 29, 72,
                Contribution("R2.02", "Dev. d'apps avec IHM", 19.50, 3),
                Contribution("R2.03", "Qualité de dev.", 13.88, 6),
                Contribution("R2.07", "Graphes", 10.00, 6),
                Contribution("R2.10", "Gestion proj. orga.", 8.23, 30),
                Contribution("R2.12", "Anglais entreprise", 14.43, 6),
                Contribution("R2.13", "Comm. prof.", 14.29, 9),
                Contribution("SAE2.05", "Gestion d'un projet", 12.75, 38),
                Contribution("P2.01", "Portfolio", 16.33, 2)),
            Unit("UE2.6", "#16A34A", "Identifier ses aptitudes pour travailler dans une équipe",
                "Collaborer", 15.14, 6, 71,
                Contribution("R2.02", "Dev. d'apps avec IHM", 19.50, 4),
                Contribution("R2.11", "Droit", 11.12, 17),
                Contribution("R2.12", "Anglais entreprise", 14.43, 17),
                Contribution("R2.13", "Comm. prof.", 14.29, 11),
                Contribution("R2.14", "PPP", 17.00, 11),
                Contribution("SAE2.06", "Organisation d'un travail d'équipe", 16.44, 38),
                Contribution("P2.01", "Portfolio", 16.33, 2))
        };

        data.Competences = new ObservableCollection<CompetenceInfo>
        {
            new() { Name = "Réaliser", Status = "ADM", UnitCode = "UE2.1" },
            new() { Name = "Optimiser", Status = "ADM", UnitCode = "UE2.2" },
            new() { Name = "Administrer", Status = "ADM", UnitCode = "UE2.3" },
            new() { Name = "Gérer", Status = "ADM", UnitCode = "UE2.4" },
            new() { Name = "Conduire", Status = "ADM", UnitCode = "UE2.5" },
            new() { Name = "Collaborer", Status = "ADM", UnitCode = "UE2.6" }
        };

        data.Resources = new ObservableCollection<EvaluationGroupInfo>
        {
            Group("R2.01", "Dev. objets", 19.13,
                Eval("évaluation n°1", 20.00, 1),
                Eval("évaluation n°2", 19.33, 2),
                Eval("évaluation n°3", 18.50, 2)),
            Group("R2.02", "Dev. d'apps avec IHM", 19.50,
                Eval("évaluation n°1", 19.50, 1)),
            Group("R2.03", "Qualité de dev.", 14.06,
                Eval("tests unitaires", 13.12, 1.5),
                Eval("qcm git", 15.00, 1)),
            Group("R2.04", "Réseau & bas niveau", 10.00,
                Eval("DS Mécanismes de Bas Niveau", 15.00, 1),
                Eval("Réseaux et Bas Niveau", 5.00, 1)),
            Group("R2.05", "Services réseau", 12.00),
            Group("R2.06", "Exploitation BD", 11.25,
                Eval("Contrôle écrit", 18.00, 1),
                Eval("Contrôle écrit final", 4.50, 1)),
            Group("R2.07", "Graphes", 10.00,
                Eval("Contrôle 1", 9.75, 1),
                Eval("C2", 10.25, 1)),
            Group("R2.08", "Stats descriptives", 4.00,
                Eval("DS Statistiques", 4.00, 1)),
            Group("R2.09", "Méthodes numériques", 9.25,
                Eval("Contrôle 1", 11.75, 1),
                Eval("DS", 6.75, 1)),
            Group("R2.10", "Gestion proj. orga.", 8.23,
                Eval("WBS - Organigramme des tâches - Bomber IUT", 17.50, 0.25),
                Eval("DS méthodes de planification projet", 17.00, 1),
                Eval("DS2 gestion de projet", 9.77, 1),
                Eval("DS1 Compta-finance", 2.75, 1),
                Eval("DS2 Compta-finance", 1.06, 1)),
            Group("R2.11", "Droit", 11.12,
                Eval("Contrôle 1", 11.25, 1),
                Eval("Contrôle 2", 11.00, 1)),
            Group("R2.12", "Anglais entreprise", 14.43,
                EvalText("VOCAB TEST", "EXC", 1),
                EvalText("TELEPHONING", "EXC", 1),
                EvalText("DS", "EXC", 1),
                EvalText("Application CV-Cover Letters", "EXC", 1),
                EvalText("Application - Job Interview - Jury", "EXC", 1),
                EvalText("JOB AD", "EXC", 1),
                Eval("moyenne groupe B", 14.43, 1)),
            Group("R2.13", "Comm. prof.", 14.29,
                Eval("cont exposes", 17.00, 1),
                Eval("résumé", 14.00, 1),
                Eval("Évaluation", 16.00, 1),
                Eval("Évaluation", 13.00, 3)),
            Group("R2.14", "PPP", 17.00,
                Eval("PPP-Portfolio S2 (sur ecampus-eportfolio)", 17.00, 1)),
            Group("P2.01", "Portfolio", 16.33,
                Eval("PPP-Portfolio S2 (sur ecampus-eportfolio)", 17.00, 1),
                Eval("Devoir écrit de synthèse", 16.00, 2))
        };

        data.Saes = new ObservableCollection<EvaluationGroupInfo>
        {
            Group("SAE2.01", "Développement d'une application", 18.25,
                Eval("évaluation écrite", 18.50, 1),
                Eval("soutenance + rapport + réalisation", 18.00, 1)),
            Group("SAE2.02", "Exploration algorithmique d'un problème", 15.60,
                Eval("évaluation écrite", 9.00, 2),
                Eval("soutenance + rapport", 20.00, 3)),
            Group("SAE2.03", "Installation de services réseau", 14.00),
            Group("SAE2.04", "Exploitation BD", 13.88,
                Eval("SAé statistique BD", 7.50, 1),
                Eval("Restitution - Partie Postgres", 16.00, 3),
                EvalText("SAE Exploitation BD Anglais", "EXC", 1)),
            Group("SAE2.05", "Gestion d'un projet", 12.75,
                Eval("Saé Méthodologie de gestion de projet", 12.75, 1)),
            Group("SAE2.06", "Organisation d'un travail d'équipe", 16.44,
                Eval("DST Communication - Partie individuelle", 16.00, 1),
                Eval("DST Communication - Partie en équipe", 16.67, 1),
                Eval("DST Economie-Gestion - Partie individuelle", 16.67, 1))
        };

        return data;
    }

    private static TeachingUnitInfo Unit(string code, string color, string name, string shortName,
        double average, int rank, int total, params ContributionInfo[] contributions)
    {
        var unit = new TeachingUnitInfo
        {
            Code = code,
            ColorHex = color,
            Name = name,
            ShortName = shortName,
            Average = GradeValue.Numeric(average),
            Rank = rank,
            Total = total,
            Status = "ADM",
            Contributions = new ObservableCollection<ContributionInfo>(contributions)
        };
        unit.NormalizeWeights();
        return unit;
    }

    private static ContributionInfo Contribution(string code, string label, double grade, double coef) => new()
    {
        Code = code,
        Label = label,
        Grade = GradeValue.Numeric(grade),
        Coefficient = coef
    };

    private static EvaluationGroupInfo Group(string code, string label, double average,
        params EvaluationInfo[] evaluations) => new()
    {
        Code = code,
        Label = label,
        Average = GradeValue.Numeric(average),
        Evaluations = new ObservableCollection<EvaluationInfo>(evaluations)
    };

    private static EvaluationInfo Eval(string name, double grade, double coef) => new()
    {
        Name = name,
        Grade = GradeValue.Numeric(grade),
        Coefficient = coef
    };

    private static EvaluationInfo EvalText(string name, string token, double coef) => new()
    {
        Name = name,
        Grade = GradeValue.Text(token),
        Coefficient = coef
    };
}
