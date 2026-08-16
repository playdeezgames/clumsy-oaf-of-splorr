Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence

Public Module ItemVerbExtensions
    Private Delegate Function CanPerformHandler(verb As IVerb, item As IItem, actor As ICharacter) As Boolean
    Private Delegate Sub PerformHandler(verb As IVerb, item As IItem, actor As ICharacter)
#Region "Can Perform"
    Private ReadOnly canPerformTable As New Dictionary(Of String, CanPerformHandler) From
        {
        }

    <Extension>
    Public Function CanPerform(verb As IVerb, item As IItem, actor As ICharacter) As Boolean
        Dim handler As CanPerformHandler = Nothing
        If canPerformTable.TryGetValue(verb.EntitySubtype, handler) Then
            Return handler.Invoke(verb, item, actor)
        End If
        Return True
    End Function
#End Region
#Region "Perform"
    Private ReadOnly performTable As New Dictionary(Of String, PerformHandler) From
        {
            {VerbSubtypes.SELL_CAKE, AddressOf HandleSellCake}
        }

    Private Sub HandleSellCake(verb As IVerb, item As IItem, actor As ICharacter)
        Dim layers = item.GetCounter(Counters.LAYERS)
        Dim jools = Grimoire.JOOLS_PER_LAYER * layers
        actor.AddMessage($"{actor.Name} sells {layers} layer cake for {jools:F2} jools.")
        actor.ChangeDimension(Dimensions.JOOLS, jools)
        actor.AddMessage($"{actor.Name} now has {actor.GetDimension(Dimensions.JOOLS):F2} jools.")
        item.Remove()
    End Sub

    <Extension>
    Sub Perform(verb As IVerb, item As IItem, actor As ICharacter)
        Dim handler As PerformHandler = Nothing
        If performTable.TryGetValue(verb.EntitySubtype, handler) Then
            handler.Invoke(verb, item, actor)
            Return
        End If
    End Sub
#End Region
End Module
