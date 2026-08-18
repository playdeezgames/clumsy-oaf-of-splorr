Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence

Public Module FeatureVerbExtensions
    Private Delegate Function CanPerformHandler(verb As IVerb, feature As IFeature, actor As ICharacter) As Boolean
    Private Delegate Sub PerformHandler(verb As IVerb, feature As IFeature, actor As ICharacter)
#Region "Can Perform"
    Private ReadOnly canPerformTable As New Dictionary(Of String, CanPerformHandler) From
        {
            {VerbSubtypes.SET_CHECKPOINT, AddressOf CanSetCheckpoint}
        }

    Private Function CanSetCheckpoint(verb As IVerb, feature As IFeature, actor As ICharacter) As Boolean
        Return actor.IsAvatar() AndAlso Not actor.IsCurrentCheckpoint(feature)
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
            {VerbSubtypes.SET_CHECKPOINT, AddressOf HandleSetCheckpoint},
            {VerbSubtypes.TOUCH, AddressOf HandleTouch},
            {VerbSubtypes.ENTER, AddressOf HandleEnter}
        }

    Private Sub HandleEnter(verb As IVerb, feature As IFeature, actor As ICharacter)
        Dim nextLocation = feature.GetDestination()
        actor.AddMessage($"{actor.Name} goes through {feature.Name}.")
        actor.Location = nextLocation
        actor.Look()
    End Sub
#Region "Touch"
    Private ReadOnly touchHandlers As New Dictionary(Of String, PerformHandler) From
        {
            {FeatureSubtypes.CACTUS, AddressOf HandleTouchCactus}
        }
    Private Sub HandleTouchCactus(verb As IVerb, feature As IFeature, actor As ICharacter)
        actor.AddMessage($"{actor.Name} feels a prick.")
        actor.Die()
    End Sub
    Private Sub HandleTouch(verb As IVerb, feature As IFeature, actor As ICharacter)
        actor.AddMessage($"{actor.Name} touches {feature.Name}.")
        Dim touchHandler As PerformHandler = Nothing
        If touchHandlers.TryGetValue(feature.EntitySubtype, touchHandler) Then
            touchHandler.Invoke(verb, feature, actor)
        End If
    End Sub

#End Region

    Private Sub HandleSetCheckpoint(verb As IVerb, feature As IFeature, actor As ICharacter)
        actor.AddMessage($"{actor.Name} sets checkpoint.")
        actor.SetCheckpoint(feature)
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
