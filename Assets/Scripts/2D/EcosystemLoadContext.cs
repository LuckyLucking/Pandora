public static class EcosystemLoadContext
{
    public static EcosystemSetup SelectedEcosystem { get; private set; }

    public static void Select(EcosystemSetup ecosystemSetup)
    {
        SelectedEcosystem = ecosystemSetup;
    }

    public static EcosystemSetup ConsumeSelectedEcosystem()
    {
        EcosystemSetup selectedEcosystem = SelectedEcosystem;
        SelectedEcosystem = null;
        return selectedEcosystem;
    }
}
