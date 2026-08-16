Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence

Public Module FeatureExtensions
#Region "Describe"
    Private Sub DescribeFeature(feature As IFeature)
        feature.AddMessage($"It is a {feature.Name}.")
    End Sub
    Private Delegate Sub FeatureDescriber(feature As IFeature)
    Private ReadOnly featureDescribers As New Dictionary(Of String, FeatureDescriber) From
        {
            {FeatureSubtypes.OVEN, AddressOf DescribeOven},
            {FeatureSubtypes.DRY_PANTRY, AddressOf DescribeDryPantry},
            {FeatureSubtypes.REFRIGERATOR, AddressOf DescribeRefrigerator}
        }

    Private Sub DescribeRefrigerator(feature As IFeature)
        DescribeFeature(feature)
        feature.AddMessage($"Butter: {feature.GetCounterStatistic(Counters.BUTTER)}")
        feature.AddMessage($"Eggs: {feature.GetCounterStatistic(Counters.EGG)}")
        feature.AddMessage($"Milk: {feature.GetCounterStatistic(Counters.MILK)}")
    End Sub

    Private Sub DescribeDryPantry(feature As IFeature)
        DescribeFeature(feature)
        feature.AddMessage($"Baking Powder: {feature.GetCounterStatistic(Counters.BAKING_POWDER)}")
        feature.AddMessage($"Baking Soda: {feature.GetCounterStatistic(Counters.BAKING_SODA)}")
        feature.AddMessage($"Flour: {feature.GetCounterStatistic(Counters.FLOUR)}")
        feature.AddMessage($"Salt: {feature.GetCounterStatistic(Counters.SALT)}")
        feature.AddMessage($"Sugar: {feature.GetCounterStatistic(Counters.SUGAR)}")
        feature.AddMessage($"Vanilla: {feature.GetCounterStatistic(Counters.VANILLA)}")
    End Sub

    Private Sub DescribeOven(feature As IFeature)
        DescribeFeature(feature)
        If feature.HasTag(Tags.ON) Then
            feature.AddMessage($"{feature.Name} is on.")
        End If
        If feature.HasTag(Tags.OPEN) Then
            feature.AddMessage($"{feature.Name}'s door is open.")
        End If
        If feature.Inventory.HasItemOfSubtype(ItemSubtypes.CAKE_PAN) Then
            Dim item = feature.Inventory.GetItemsOfSubtype(ItemSubtypes.CAKE_PAN).Single()
            feature.AddMessage($"{feature.Name} has {item.Name} in it.")
        End If
    End Sub

    <Extension>
    Public Sub Describe(feature As IFeature)
        Dim describer As FeatureDescriber = Nothing
        If featureDescribers.TryGetValue(feature.EntitySubtype, describer) Then
            describer.Invoke(feature)
        Else
            DescribeFeature(feature)
        End If
    End Sub
#End Region
#Region "Destination"
    <Extension>
    Public Sub SetDestination(feature As IFeature, destination As ILocation)
        feature.SetYoke(Yokes.DESTINATION, destination.EntityId)
    End Sub
    <Extension>
    Public Function GetDestination(feature As IFeature) As ILocation
        Return feature.World.GetLocation(feature.GetYoke(Yokes.DESTINATION))
    End Function
#End Region
#Region "Verbs"
    <Extension>
    Public Sub CreateEnterVerb(feature As IFeature)
        feature.CreateVerb(VerbSubtypes.ENTER, "Enter")
    End Sub
    <Extension>
    Public Sub CreateSleepVerb(feature As IFeature)
        feature.CreateVerb(VerbSubtypes.SLEEP, "Sleep")
    End Sub
#End Region
#Region "Computer Prices"
    Private ReadOnly prices As New List(Of (Name As String, Price As Double, FeatureSubtype As String, CounterId As String)) From
        {
            ("Buy Baking Powder", 2.0, FeatureSubtypes.DRY_PANTRY, Counters.BAKING_POWDER),
            ("Buy Baking Soda", 2.0, FeatureSubtypes.DRY_PANTRY, Counters.BAKING_SODA),
            ("Buy Flour", 5.0, FeatureSubtypes.DRY_PANTRY, Counters.FLOUR),
            ("Buy Salt", 1.0, FeatureSubtypes.DRY_PANTRY, Counters.SALT),
            ("Buy Sugar", 10.0, FeatureSubtypes.DRY_PANTRY, Counters.SUGAR),
            ("Buy Vanilla", 25.0, FeatureSubtypes.DRY_PANTRY, Counters.VANILLA),
            ("Buy Butter", 15.0, FeatureSubtypes.REFRIGERATOR, Counters.BUTTER),
            ("Buy Eggs", 10.0, FeatureSubtypes.REFRIGERATOR, Counters.EGG),
            ("Buy Milk", 10.0, FeatureSubtypes.REFRIGERATOR, Counters.MILK)
        }
    <Extension>
    Public Sub AddPrices(feature As IFeature)
        For Each price In prices
            feature.CreateVerb(VerbSubtypes.BUY_SUPPLIES, $"{price.Name}({price.Price:f2} jools)", InitializeBuySupplies(price.FeatureSubtype, price.CounterId, price.Price))
        Next
        feature.CreateVerb(VerbSubtypes.BUY_CAKE_BOARD, "Buy Cake Board(1.00 jools)")
    End Sub

    Private Function InitializeBuySupplies(featureSubtype As String, counterId As String, price As Double) As VerbInitializer
        Return Sub(verb)
                   verb.SetMetadata(Metadatas.FEATURE_SUBTYPE, featureSubtype)
                   verb.SetMetadata(Metadatas.COUNTER_ID, counterId)
                   verb.SetDimension(Dimensions.JOOLS, price)
               End Sub
    End Function
#End Region
End Module
