using Optional;

namespace DidacticalEnigma.Core.Models.Project
{
    public class ModificationResult
    {
        public Option<Translation> Translation { get; }

        public bool IsSuccessful => Translation.HasValue;

        public Option<(Translation updatedDestination, Translation attemptedModification)> Conflict { get; }

        public Option<string> ReasonForBeingUnsupported { get; }

        public static ModificationResult WithSuccess(Translation translation)
        {
            return new ModificationResult(translation.Some(), Option.None<(Translation updatedDestination, Translation attemptedModification)>(), Option.None<string>());
        }

        private ModificationResult(Option<Translation> translation, Option<(Translation updatedDestination, Translation attemptedModification)> conflict, Option<string> reasonForBeingUnsupported)
        {
            Translation = translation;
            Conflict = conflict;
            ReasonForBeingUnsupported = reasonForBeingUnsupported;
        }

        public static ModificationResult WithUnsupported(string reason)
        {
            return new ModificationResult(Option.None<Translation>(), new Option<(Translation updatedDestination, Translation attemptedModification)>(), Option.Some(reason));
        }

        public static ModificationResult WithConflict(Translation updatedDestination, Translation attemptedModification)
        {
            return new ModificationResult(Option.None<Translation>(), Option.Some<(Translation, Translation)>((updatedDestination, attemptedModification)), Option.None<string>());
        }
    }
}