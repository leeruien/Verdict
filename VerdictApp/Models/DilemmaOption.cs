namespace VerdictApp.Models;
public class DilemmaOption {
    public Guid Id { get; set; }
    public Guid DilemmaId { get; set; }
    public string OptionText { get; set; }
    public Dilemma Dilemma { get; set; }
    public List<Vote> Votes { get; set; }
}