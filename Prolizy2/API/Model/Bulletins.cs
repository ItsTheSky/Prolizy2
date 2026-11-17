using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Prolizy2.API.Utils.Converters;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace Prolizy2.API.Model;

public sealed class BulletinRoot
{
    [JsonPropertyName("config")] public Configuration Config { get; init; }
    [JsonPropertyName("auth")] public Authentication Auth { get; init; }
    [JsonPropertyName("semestres")] public List<Semester> Semesters { get; init; }
    [JsonPropertyName("relevé")] public Transcript Transcript { get; init; }
    // Absences come back in inconsistent shapes (empty array vs object vs nested list);
    // we keep the raw payload here and parse it on demand to avoid breaking the whole load.
    [JsonPropertyName("absences")] public JsonElement? Absences { get; init; }
}

public sealed class Configuration
{
    [JsonPropertyName("passerelle_version")] public string GatewayVersion { get; init; }
    [JsonPropertyName("histogramme")] public bool ShowHistogram { get; init; }
    [JsonPropertyName("message_non_publication_releve")] public string UnpublishedTranscriptMessage { get; init; }
    [JsonPropertyName("releve_PDF")] public bool EnablePdfTranscript { get; init; }
    [JsonPropertyName("liste_dep_publi_PDF")] public string PdfPublishingDepartments { get; init; }
    [JsonPropertyName("liste_dep_affichage_appreciations")] public string AppreciationDisplayDepartments { get; init; }
    [JsonPropertyName("etudiant_modif_photo")] public bool AllowStudentPhotoModification { get; init; }
    [JsonPropertyName("acces_enseignants")] public bool TeacherAccess { get; init; }
    [JsonPropertyName("cloisonner_enseignants")] public bool PartitionTeachers { get; init; }
    [JsonPropertyName("analystics_interne")] public bool InternalAnalytics { get; init; }
    [JsonPropertyName("envoi_donnees_version")] public bool SendVersionData { get; init; }
    [JsonPropertyName("analyse_temps_requetes")] public bool AnalyzeRequestTimes { get; init; }
    [JsonPropertyName("nom_IUT")] public string InstituteName { get; init; }
    [JsonPropertyName("doc_afficher_nip")] public bool DisplayNip { get; init; }
    [JsonPropertyName("doc_afficher_id")] public bool DisplayId { get; init; }
    [JsonPropertyName("doc_afficher_date_naissance")] public bool DisplayBirthDate { get; init; }
    [JsonPropertyName("idReg")] public string IdRegex { get; init; }
    [JsonPropertyName("idPlaceHolder")] public string IdPlaceholder { get; init; }
    [JsonPropertyName("idInfo")] public string IdInfo { get; init; }
    [JsonPropertyName("nameReg")] public string NameRegex { get; init; }
    [JsonPropertyName("namePlaceHolder")] public string NamePlaceholder { get; init; }
    [JsonPropertyName("nameInfo")] public string NameInfo { get; init; }
    [JsonPropertyName("module_absences")] public bool EnableAbsenceModule { get; init; }
    [JsonPropertyName("afficher_absences")] public bool ShowAbsences { get; init; }
    [JsonPropertyName("data_absences_scodoc")] public bool UseScodocAbsenceData { get; init; }
    [JsonPropertyName("metrique_absences")] public string AbsenceMetric { get; init; }
    [JsonPropertyName("autoriser_justificatifs")] public bool AllowJustifications { get; init; }
    [JsonPropertyName("liste_dep_ok_justificatifs")] public string JustificationDepartments { get; init; }
    [JsonPropertyName("liste_dep_publi_absences")] public string AbsencePublishingDepartments { get; init; }
    [JsonPropertyName("message_rapport_absences")] public string AbsenceReportMessage { get; init; }
    [JsonPropertyName("message_justificatifs")] public string JustificationMessage { get; init; }
    [JsonPropertyName("absence_heureDebut")] public double AbsenceStartHour { get; init; }
    [JsonPropertyName("absence_heureFin")] public double AbsenceEndHour { get; init; }
    [JsonPropertyName("absence_pas")] public double AbsenceStep { get; init; }
    [JsonPropertyName("absence_dureeSeance")] public double AbsenceSessionDuration { get; init; }
    [JsonPropertyName("session")] public string Session { get; init; }
    [JsonPropertyName("name")] public string Name { get; init; }
    [JsonPropertyName("statut")] public int Status { get; init; }
}

public sealed class Authentication 
{
    [JsonPropertyName("session")] public string SessionId { get; init; }
    [JsonPropertyName("name")] public string Name { get; init; }
    [JsonPropertyName("statut")] public int Status { get; init; }
}

public sealed class Semester 
{
    [JsonPropertyName("titre")] public string Title { get; init; }
    [JsonPropertyName("formsemestre_id")] public int SemesterId { get; init; }
    [JsonPropertyName("semestre_id")] public int SemesterNumber { get; init; }
    [JsonPropertyName("annee_scolaire")] public string AcademicYear { get; init; }
}

public sealed class Transcript
{
    [JsonPropertyName("version")] public string Version { get; init; }
    [JsonPropertyName("type")] public string Type { get; init; }
    [JsonPropertyName("date")] public DateTime Date { get; init; }
    [JsonPropertyName("publie")] public bool IsPublished { get; init; }
    [JsonPropertyName("etat_inscription")] public string EnrollmentStatus { get; init; }
    [JsonPropertyName("etudiant")] public Student Student { get; init; }
    [JsonPropertyName("formation")] public Formation Formation { get; init; }
    [JsonPropertyName("formsemestre_id")] public int SemesterId { get; init; }
    [JsonPropertyName("options")] public TranscriptOptions Options { get; init; }
    [JsonPropertyName("appreciation")] public List<JsonElement> Appreciations { get; init; }
    [JsonPropertyName("ressources")] public Dictionary<string, Resource> Resources { get; init; }
    [JsonPropertyName("saes")] public Dictionary<string, Sae> Saes { get; init; }
    [JsonPropertyName("ues_capitalisees")] public Dictionary<string, JsonElement> CapitalisedUnits { get; init; }
    [JsonPropertyName("semestre")] public TranscriptSemester Semester { get; init; }
    [JsonPropertyName("ues")] public Dictionary<string, TeachingUnit> TeachingUnits { get; init; }
}

public sealed class Sae
{
    
    [JsonPropertyName("id")] public int Id { get; init; }
    [JsonPropertyName("titre")] public string Title { get; init; }
    [JsonPropertyName("code_apogee")] public string ApogeeCode { get; init; }
    [JsonPropertyName("url")] public string Url { get; init; }
    [JsonPropertyName("moyenne")] public Grade Average { get; init; }
    [JsonPropertyName("evaluations")] public List<Evaluation> Evaluations { get; init; }
    
}

public sealed class Student
{
    [JsonPropertyName("boursier")] public string Scholarship { get; init; }
    [JsonPropertyName("civilite_etat_civil")] public string? CivilityLegal { get; init; }
    [JsonPropertyName("civilite")] public string Civility { get; init; }
    [JsonPropertyName("code_ine")] public string IneCode { get; init; }
    [JsonPropertyName("code_nip")] public string StudentId { get; init; }
    [JsonPropertyName("date_naissance")] public string BirthDate { get; init; }
    [JsonPropertyName("dept_acronym")] public string DepartmentAcronym { get; init; }
    [JsonPropertyName("dept_id")] public int DepartmentId { get; init; }
    [JsonPropertyName("dept_naissance")] public string BirthDepartment { get; init; }
    [JsonPropertyName("email")] public string Email { get; init; }
    [JsonPropertyName("emailperso")] public string PersonalEmail { get; init; }
    [JsonPropertyName("etat_civil")] public string LegalStatus { get; init; }
    [JsonPropertyName("etudid")] public int StudentDbId { get; init; }
    [JsonPropertyName("lieu_naissance")] public string BirthPlace { get; init; }
    [JsonPropertyName("nationalite")] public string Nationality { get; init; }
    [JsonPropertyName("nom")] public string LastName { get; init; }
    [JsonPropertyName("nomprenom")] public string FullNameFormatted { get; init; }
    [JsonPropertyName("prenom_etat_civil")] public string? FirstNameLegal { get; init; }
    [JsonPropertyName("prenom")] public string FirstName { get; init; }
    [JsonPropertyName("fiche_url")] public string ProfileUrl { get; init; }
    [JsonPropertyName("photo_url")] public string PhotoUrl { get; init; }
    [JsonPropertyName("domicile")] public string Address { get; init; }
    [JsonPropertyName("villedomicile")] public string City { get; init; }
    [JsonPropertyName("codepostaldomicile")] public string PostalCode { get; init; }
    [JsonPropertyName("paysdomicile")] public string Country { get; init; }
    [JsonPropertyName("telephone")] public string Phone { get; init; }
    [JsonPropertyName("telephonemobile")] public string MobilePhone { get; init; }
    [JsonPropertyName("fax")] public string Fax { get; init; }
    [JsonPropertyName("typeadresse")] public string AddressType { get; init; }
    [JsonPropertyName("id")] public int Id { get; init; }
    [JsonPropertyName("description")] public string Description { get; init; }

    [JsonIgnore] public string FullName => $"{FirstName} {LastName}";
}

public sealed class Resource
{
    [JsonPropertyName("id")] public int Id { get; init; }
    [JsonPropertyName("titre")] public string Title { get; init; }
    [JsonPropertyName("code_apogee")] public string ApogeeCode { get; init; }
    [JsonPropertyName("url")] public string Url { get; init; }
    [JsonPropertyName("moyenne")] public Grade Average { get; init; }
    [JsonPropertyName("evaluations")] public List<Evaluation> Evaluations { get; init; }
}

public sealed class Grade 
{
    [JsonPropertyName("value")] public string Value { get; init; }
    [JsonPropertyName("min")] public string Minimum { get; init; }
    [JsonPropertyName("max")] public string Maximum { get; init; }
    [JsonPropertyName("moy")] public string Average { get; init; }
    
    [JsonPropertyName("rang")] public string Rank { get; init; }
    [JsonPropertyName("total")] public int TotalRank { get; init; }
}

public sealed class Evaluation
{
    [JsonPropertyName("id")] public int Id { get; init; }
    [JsonPropertyName("coef")] public string Coefficient { get; init; }
    [JsonPropertyName("date_debut")] public DateTime DateStart { get; init; }
    [JsonPropertyName("date_fin")] public DateTime DateEnd { get; init; }
    [JsonPropertyName("description")] public string Description { get; init; }
    [JsonPropertyName("evaluation_type")] public int EvaluationType { get; init; }
    [JsonPropertyName("note")] public Grade Grade { get; init; }
    // Weights and coefficients are decimal on the server side (0.5, 1.5, ...).
    [JsonPropertyName("poids")] public Dictionary<string, double> Weights { get; init; }
    [JsonPropertyName("url")] public string Url { get; init; }
    [JsonPropertyName("date")] public DateTime Date { get; init; }
    [JsonPropertyName("heure_debut")] public string StartTime { get; init; }
    [JsonPropertyName("heure_fin")] public string EndTime { get; init; }

    public async Task<List<double>?> FetchNotes(BulletinClient client) => await client.FetchListNotes(this);
}

public sealed class TeachingUnit
{
    [JsonPropertyName("id")] public int Id { get; init; }
    [JsonPropertyName("titre")] public string Title { get; init; }
    [JsonPropertyName("numero")] public int Number { get; init; }
    [JsonPropertyName("type")] public int Type { get; init; }
    [JsonPropertyName("color")] public string Color { get; init; }
    [JsonPropertyName("competence")] public string? Competence { get; init; }
    [JsonPropertyName("moyenne")] public Grade Average { get; init; }
    [JsonPropertyName("bonus")] public string Bonus { get; init; }
    [JsonPropertyName("malus")] public string Malus { get; init; }
    [JsonPropertyName("capitalise")] public string? Capitalised { get; init; }
    [JsonPropertyName("ressources")] public Dictionary<string, TeachingUnitEntry> Resources { get; init; }
    [JsonPropertyName("saes")] public Dictionary<string, TeachingUnitEntry> Saes { get; init; }
}

public sealed class TeachingUnitEntry
{
    [JsonPropertyName("id")] public int Id { get; init; }
    [JsonPropertyName("coef")] public double Coefficient { get; init; }
    [JsonPropertyName("moyenne")] public string Average { get; init; }
}

public sealed class UnitResource
{
    [JsonPropertyName("id")] public int Id { get; init; }
    [JsonPropertyName("coef")] public double Coefficient { get; init; }
    [JsonPropertyName("moyenne")] public string Average { get; init; }
}

public sealed class Formation
{
    [JsonPropertyName("id")] public int Id { get; init; }
    [JsonPropertyName("acronyme")] public string Acronym { get; init; }
    [JsonPropertyName("titre_officiel")] public string OfficialTitle { get; init; }
    [JsonPropertyName("titre")] public string Title { get; init; }
}

public sealed class TranscriptOptions
{
    [JsonPropertyName("show_abs")] public bool ShowAbsences { get; init; }
    [JsonPropertyName("show_abs_modules")] public bool ShowAbsencesPerModule { get; init; }
    [JsonPropertyName("show_ects")] public bool ShowEcts { get; init; }
    [JsonPropertyName("show_codemodules")] public bool ShowModuleCodes { get; init; }
    [JsonPropertyName("show_matieres")] public bool ShowSubjects { get; init; }
    [JsonPropertyName("show_rangs")] public bool ShowRanks { get; init; }
    [JsonPropertyName("show_ue_rangs")] public bool ShowUeRanks { get; init; }
    [JsonPropertyName("show_mod_rangs")] public bool ShowModuleRanks { get; init; }
    [JsonPropertyName("show_moypromo")] public bool ShowPromoAverage { get; init; }
    [JsonPropertyName("show_moyenne_generale_but")] public bool ShowGeneralAverageBut { get; init; }
    [JsonPropertyName("show_minmax")] public bool ShowMinMax { get; init; }
    [JsonPropertyName("show_minmax_mod")] public bool ShowMinMaxPerModule { get; init; }
    [JsonPropertyName("show_minmax_eval")] public bool ShowMinMaxPerEvaluation { get; init; }
    [JsonPropertyName("show_coef")] public bool ShowCoefficients { get; init; }
    [JsonPropertyName("show_ue_cap_details")] public bool ShowUeCapDetails { get; init; }
    [JsonPropertyName("show_ue_cap_current")] public bool ShowUeCapCurrent { get; init; }
    [JsonPropertyName("show_temporary")] public bool ShowTemporary { get; init; }
    [JsonPropertyName("temporary_txt")] public string TemporaryText { get; init; }
    [JsonPropertyName("show_uevalid")] public bool ShowUeValidation { get; init; }
    [JsonPropertyName("show_date_inscr")] public bool ShowEnrollmentDate { get; init; }
    [JsonPropertyName("block_moyenne_generale")] public bool BlockGeneralAverage { get; init; }
    [JsonPropertyName("bgcolor")] public string BackgroundColor { get; init; }
}

public sealed class TranscriptSemester
{
    [JsonPropertyName("etapes")] public List<string> Steps { get; init; }
    [JsonPropertyName("date_debut")] public string StartDate { get; init; }
    [JsonPropertyName("date_fin")] public string EndDate { get; init; }
    [JsonPropertyName("annee_universitaire")] public string AcademicYear { get; init; }
    [JsonPropertyName("numero")] public int Number { get; init; }
    [JsonPropertyName("inscription")] public string Enrollment { get; init; }
    [JsonPropertyName("groupes")] public List<Group> Groups { get; init; }
    [JsonPropertyName("badges")] public List<JsonElement> Badges { get; init; }
    [JsonPropertyName("absences")] public SemesterAbsences Absences { get; init; }
    [JsonPropertyName("diplomation")] public string Diplomation { get; init; }
    [JsonPropertyName("situation")] public string Situation { get; init; }
    [JsonPropertyName("diplome_dut120")] public bool DiplomedDut120 { get; init; }
    [JsonPropertyName("diplome_dut120_descr")] public string DiplomedDut120Description { get; init; }
    [JsonPropertyName("decision")] public SemesterDecision Decision { get; init; }
    [JsonPropertyName("decision_ue")] public List<JsonElement> DecisionUe { get; init; }
    [JsonPropertyName("autorisation_inscription")] public List<JsonElement> EnrollmentAuthorizations { get; init; }
    [JsonPropertyName("decision_rcue")] public List<JsonElement> DecisionRcue { get; init; }
    [JsonPropertyName("descr_decisions_rcue")] public string RcueDecisionsDescription { get; init; }
    [JsonPropertyName("descr_decisions_rcue_list")] public List<JsonElement> RcueDecisionsList { get; init; }
    [JsonPropertyName("descr_decisions_niveaux")] public string LevelDecisionsDescription { get; init; }
    [JsonPropertyName("decision_annee")] public JsonElement? YearDecision { get; init; }
    [JsonPropertyName("notes")] public Grade Notes { get; init; }
    [JsonPropertyName("rang")] public Grade Rank { get; init; }
}

public sealed class Group
{
    [JsonPropertyName("group_name")] public string Name { get; init; }
    [JsonPropertyName("partition_id")] public int PartitionId { get; init; }
    [JsonPropertyName("numero")] public int Number { get; init; }
    [JsonPropertyName("id")] public int Id { get; init; }
    [JsonPropertyName("edt_id")] public string? EdtId { get; init; }
    [JsonPropertyName("partition")] public Partition Partition { get; init; }
}

public sealed class Partition
{
    [JsonPropertyName("id")] public int Id { get; init; }
    [JsonPropertyName("bul_show_rank")] public bool ShowRankInBulletin { get; init; }
    [JsonPropertyName("groups_editable")] public bool GroupsEditable { get; init; }
    [JsonPropertyName("formsemestre_id")] public int SemesterId { get; init; }
    [JsonPropertyName("partition_name")] public string Name { get; init; }
    [JsonPropertyName("numero")] public int Number { get; init; }
    [JsonPropertyName("show_in_lists")] public bool ShowInLists { get; init; }
    [JsonPropertyName("partition_id")] public int PartitionId { get; init; }
}

public sealed class SemesterAbsences
{
    // Counted in the unit given by "metrique" (half-days, hours...), so not always whole numbers.
    [JsonPropertyName("injustifie")] public double Unjustified { get; init; }
    [JsonPropertyName("total")] public double Total { get; init; }
    [JsonPropertyName("metrique")] public string Metric { get; init; }
}

public sealed class SemesterDecision
{
    [JsonPropertyName("code")] public string Code { get; init; }
    [JsonPropertyName("etat")] public string State { get; init; }
    [JsonPropertyName("date")] public string Date { get; init; }
}

public sealed class Absence
{
    [JsonPropertyName("idAbs")] public int Id { get; init; }
    [JsonPropertyName("debut"), JsonConverter(typeof(WeirdTimeSpanConverter))] public TimeSpan StartTime { get; init; }
    [JsonPropertyName("fin"), JsonConverter(typeof(WeirdTimeSpanConverter))] public TimeSpan EndTime { get; init; }
    [JsonPropertyName("statut")] public string Status { get; init; }
    [JsonPropertyName("justifie")] public bool IsJustified { get; init; }
    [JsonPropertyName("enseignant")] public string Teacher { get; init; }
    [JsonPropertyName("matiereComplet")] public object Subject { get; init; }
    [JsonPropertyName("dateFin")] public string EndDate { get; init; }
}