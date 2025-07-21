namespace Dataflow.PhotoStripMaker
{
    public static class GreetingPhrases
    {
        private static Random rand = new Random();
        private static readonly List<string> Phrases =
        [
            "Wishing you continued success in your endeavors.",
            "May your quarterly reports exceed expectations.",
            "Best regards from the team.",
            "Hope your KPIs are met and exceeded.",
            "Sincerely, The Management.",
            "For internal use only.",
            "Compliance is key.",
            "Please refer to the Q3 strategic roadmap.",
            "Optimizing workflows, one step at a time.",
            "Accelerating synergy across all verticals.",
            "Good luck with your upcoming exams.",
            "Hope your surgery recovery is swift.",
            "May your code compile on the first try.",
            "Sending strength for your difficult project deadline.",
            "Remember to hydrate during your marathon training.",
            "May your thesis be accepted with flying colors.",
            "Thinking of you during this challenging period.",
            "Hope the plumbing fix holds up.",
            "For a healthier, more productive tomorrow.",
            "Wishing you a smooth tax season.",
            "Life is but a fleeting shadow.",
            "The inevitable march of time continues.",
            "Hope the existential dread passes quickly.",
            "Another day, another struggle.",
            "Embrace the void.",
            "May your troubles be fewer, eventually.",
            "The endless cycle of monotony.",
            "Considering the futility of it all.",
            "Here's to enduring.",
            "A moment's respite before the next storm.",
            "Contemplating the transient nature of perceived reality.",
            "The subjective interpretation of objective phenomena.",
            "A reflection on the recursive properties of consciousness.",
            "Where ephemerality meets eternal introspection.",
            "Your car's extended warranty has expired.",
            "Have you considered cryptocurrency?",
            "Don't forget to defrost the freezer.",
            "Beware of rogue squirrels.",
            "Your call is important to us.",
            "This message will self-destruct.",
            "Check your oil pressure soon.",
            "Wishing you excellent dental hygiene.",
            "Stay vigilant against papercuts."
        ];

        public static string PickRandomPhrase()
        {
            var index = rand.Next(Phrases.Count);
            return Phrases[index];
        }
    }
}
