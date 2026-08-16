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
        }
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
