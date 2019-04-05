using Optional;

namespace DidacticalEnigma.Core.Models.Project
{
    public class ModificationResult
    {
        public bool IsSuccessful { get; }

        public Option<(ITranslation updatedDestination, ITranslation attemptedModification)> Conflict { get; }

        public Option<string> ReasonForBeingUnsupported { get; }

        private static readonly ModificationResult withSuccess = new ModificationResult(
            isSuccessful: true,
            Option.None<(ITranslation updatedDestination, ITranslation attemptedModification)>(),
            Option.None<string>());

        public static ModificationResult WithSuccess() => withSuccess;

        private ModificationResult(bool isSuccessful, Option<(ITranslation updatedDestination, ITranslation attemptedModification)> conflict, Option<string> reasonForBeingUnsupported)
        {
            IsSuccessful = isSuccessful;
            Conflict = conflict;
            ReasonForBeingUnsupported = reasonForBeingUnsupported;
        }

        public static ModificationResult WithUnsupported(string reason)
        {
            return new ModificationResult(isSuccessful: false, new Option<(ITranslation updatedDestination, ITranslation attemptedModification)>(), Option.Some(reason));
        }

        public static ModificationResult WithConflict(ITranslation updatedDestination, ITranslation attemptedModification)
        {
            return new ModificationResult(isSuccessful: false, Option.Some<(ITranslation, ITranslation)>((updatedDestination, attemptedModification)), Option.None<string>());
        }
    }
}