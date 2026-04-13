namespace LPE.AI.LPE {
    public struct ActionProposalBuilder<TCallback> {
        internal ActionProposal<TCallback, Unit> proposal;


        internal ActionProposalBuilder(ActionProposal<TCallback, Unit> proposal) {
            this.proposal = proposal;
        }

        public ActionProposalBuilder<TCallback> ReportSatisfies<T, U>(U param) where T : ActionSpec<U>, new() {
            proposal.report.AddProposal<T, U>(param);
            return this;
        }
        public ActionProposalBuilder<TCallback> ReportSatisfies<T>() where T : ActionSpec, new() {
            proposal.report.AddProposal<T>();
            return this;
        }
    }

    public struct ActionProposalBuilder<TCallback, TParam> {
        internal ActionProposal<TCallback, TParam> proposal;


        internal ActionProposalBuilder(ActionProposal<TCallback, TParam> proposal) {
            this.proposal = proposal;
        }


        // TODO - better name?
        public ActionProposalBuilder<TCallback, TParam> ReportSatisfies<T, U>(U param) where T : ActionSpec<U>, new() {
            proposal.report.AddProposal<T, U>(param);
            return this;
        }
        public ActionProposalBuilder<TCallback, TParam> ReportSatisfies<T>() where T : ActionSpec, new() {
            proposal.report.AddProposal<T>();
            return this;
        }
    }


}
