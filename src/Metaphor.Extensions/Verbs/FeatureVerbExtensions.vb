Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence

Public Module FeatureVerbExtensions
    Private Delegate Function CanPerformHandler(verb As IVerb, feature As IFeature, actor As ICharacter) As Boolean
    Private Delegate Sub PerformHandler(verb As IVerb, feature As IFeature, actor As ICharacter)
#Region "Can Perform"
    Private ReadOnly canPerformTable As New Dictionary(Of String, CanPerformHandler) From
        {
            {VerbSubtypes.ENTER, AddressOf CanEnter},
            {VerbSubtypes.SLEEP, AddressOf CanSleep},
            {VerbSubtypes.BUY_SUPPLIES, AddressOf CanBuySupplies},
            {VerbSubtypes.BUY_CAKE_BOARD, AddressOf CanBuyCakeBoard}
        }

    Private Function CanBuyCakeBoard(verb As IVerb, feature As IFeature, actor As ICharacter) As Boolean
        Return actor.GetDimension(Dimensions.JOOLS) >= 1.0
    End Function

    Private Function CanBuySupplies(verb As IVerb, feature As IFeature, actor As ICharacter) As Boolean
        Return actor.GetDimension(Dimensions.JOOLS) >= verb.GetDimension(Dimensions.JOOLS)
    End Function

    Private Function CanSleep(verb As IVerb, feature As IFeature, actor As ICharacter) As Boolean
        Return Not actor.IsDead AndAlso actor.GetCounter(Counters.ENERGY) < actor.GetCounterMaximum(Counters.ENERGY) \ 2
    End Function

    Private Function CanEnter(verb As IVerb, feature As IFeature, actor As ICharacter) As Boolean
        Return Not actor.IsDead
    End Function

    <Extension>
    Public Function CanPerform(verb As IVerb, feature As IFeature, actor As ICharacter) As Boolean
        Dim handler As CanPerformHandler = Nothing
        If canPerformTable.TryGetValue(verb.EntitySubtype, handler) Then
            Return handler.Invoke(verb, feature, actor)
        End If
        Return True
    End Function
#End Region
#Region "Perform"
    Private ReadOnly performTable As New Dictionary(Of String, PerformHandler) From
        {
            {VerbSubtypes.ENTER, AddressOf HandleEnter},
            {VerbSubtypes.SLEEP, AddressOf HandleSleep},
            {VerbSubtypes.TURN_ON, AddressOf HandleTurnOn},
            {VerbSubtypes.TURN_OFF, AddressOf HandleTurnOff},
            {VerbSubtypes.OPEN_DOOR, AddressOf HandleOpenDoor},
            {VerbSubtypes.CLOSE_DOOR, AddressOf HandleCloseDoor},
            {VerbSubtypes.BUY_SUPPLIES, AddressOf HandleBuySupplies}
        }

    Private Sub HandleBuySupplies(verb As IVerb, feature As IFeature, actor As ICharacter)
        Dim jools = verb.GetDimension(Dimensions.JOOLS)
        actor.AddMessage($"{actor.Name} spends {jools:f2} jools")
        actor.ChangeDimension(Dimensions.JOOLS, -jools)
        actor.AddMessage($"{actor.Name} now has {actor.GetDimension(Dimensions.JOOLS):f2} jools")
        Dim targetFeature As IFeature = actor.Location.Features.Single(Function(x) x.EntitySubtype = verb.GetMetadata(Metadatas.FEATURE_SUBTYPE))
        targetFeature.MaximumCounter(verb.GetMetadata(Metadatas.COUNTER_ID))
        targetFeature.Describe()
    End Sub

    Private Sub HandleCloseDoor(verb As IVerb, feature As IFeature, actor As ICharacter)
        feature.ClearTag(Tags.OPEN)
    End Sub

    Private Sub HandleOpenDoor(verb As IVerb, feature As IFeature, actor As ICharacter)
        feature.SetTag(Tags.OPEN)
    End Sub

    Private Sub HandleTurnOff(verb As IVerb, feature As IFeature, actor As ICharacter)
        feature.ClearTag(Tags.ON)
    End Sub

    Private Sub HandleTurnOn(verb As IVerb, feature As IFeature, actor As ICharacter)
        feature.SetTag(Tags.ON)
    End Sub
    Private Sub HandleSleep(verb As IVerb, feature As IFeature, actor As ICharacter)
        actor.AddMessage($"{actor.Name} sleeps.")
        Dim energy = actor.GetCounterCapacity(Counters.ENERGY)
        actor.AddMessage($"{actor.Name} gains {energy} energy.")
        actor.ChangeCounter(Counters.ENERGY, energy)
        actor.AddMessage($"{actor.Name} now has {actor.GetCounterStatistic(Counters.ENERGY)} energy.")
    End Sub

    Private Sub HandleEnter(verb As IVerb, feature As IFeature, actor As ICharacter)
        actor.AddMessage($"{actor.Name} goes through {feature.Name}.")
        actor.DoBiology()
        actor.Location = feature.GetDestination()
        actor.Look()
    End Sub
    <Extension>
    Sub Perform(verb As IVerb, feature As IFeature, actor As ICharacter)
        Dim handler As PerformHandler = Nothing
        If performTable.TryGetValue(verb.EntitySubtype, handler) Then
            handler.Invoke(verb, feature, actor)
            Return
        End If
    End Sub
#End Region
End Module
