using Optional;

namespace DidacticalEnigma.Core.Models.Project
{
    public class ModificationResult
    {
        public Option<ITranslation> Translation { get; }

        public bool IsSuccessful => Translation.HasValue;

        public Option<(ITranslation updatedDestination, ITranslation attemptedModification)> Conflict { get; }

        public Option<string> ReasonForBeingUnsupported { get; }

        public static ModificationResult WithSuccess(ITranslation translation)
        {
            return new ModificationResult(translation.Some(), Option.None<(ITranslation updatedDestination, ITranslation attemptedModification)>(), Option.None<string>());
        }

        private ModificationResult(Option<ITranslation> translation, Option<(ITranslation updatedDestination, ITranslation attemptedModification)> conflict, Option<string> reasonForBeingUnsupported)
        {
            Translation = translation;
            Conflict = conflict;
            ReasonForBeingUnsupported = reasonForBeingUnsupported;
        }

        public static ModificationResult WithUnsupported(string reason)
        {
            return new ModificationResult(Option.None<ITranslation>(), new Option<(ITranslation updatedDestination, ITranslation attemptedModification)>(), Option.Some(reason));
        }

        public static ModificationResult WithConflict(ITranslation updatedDestination, ITranslation attemptedModification)
        {
            return new ModificationResult(Option.None<ITranslation>(), Option.Some<(ITranslation, ITranslation)>((updatedDestination, attemptedModification)), Option.None<string>());
        }
    }
}