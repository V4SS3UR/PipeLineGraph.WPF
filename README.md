# PipeLineGraph.WPF

**PipeLineGraph.WPF** is a WPF-based graphical control for visualizing and managing pipelines, inspired by the Jenkins BlueOcean interface. This project provides a customizable grid to represent nodes and segments, allowing for dynamic updates and visual representation of process flows.

<p align="center">
  <img src="https://github.com/user-attachments/assets/c6affec8-1286-4d89-bb86-0da50db2b5e2" width="40%">
  <img src="https://github.com/user-attachments/assets/20f8d971-6499-448b-8a9f-55c817c3e75a" width="42%">
</p>

## Features

- **Customizable Grid**: Define and customize nodes and segments within a grid layout.
- **Node Management**: Add, remove, and update nodes with visual feedback.
- **Segment Management**: Automatically manage segments between nodes.
- **State Management**: Change node states to reflect different stages of processing.
- **Fully Responsive**: The control adjusts seamlessly to different screen sizes and resolutions.

<p align="center">
  <img src="https://github.com/user-attachments/assets/9670bc0f-5baf-43cd-a0ef-d51d7cf7012c">
</p>


## Getting Started

### 1. Installation

To use `PipeLineGraph.WPF` in your WPF application, include it manually into your WPF project.

### 2. Create the View

Define the view in XAML to use the `PipeLineGrid` control:

```xml
<UserControl x:Class="MVVM.View.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:local="clr-namespace:MVVM.View.MainWindow"
        xmlns:pipeline="clr-namespace:PipeLineGraph">

    <Grid>
        <pipeline:PipeLineGrid NodeItemSource="{Binding Nodes}" />
    </Grid>

</UserControl>
```

### 3. Implement the ViewModel

Create a view model to manage nodes and commands. Here’s a basic example:

```csharp
public class MainWindow_ViewModel : ObservableObject
{
  public ObservableCollection<Node> Nodes { get; set; }
  
  public MainWindow_ViewModel()
  {
      Nodes = new ObservableCollection<Node>();    
  }   
}
```

## Example Processes

### Static Flow

The static flow demonstrates a predefined process where nodes and connections are established once and do not change dynamically during execution.

<p align="center">
  <img src="https://github.com/user-attachments/assets/c6affec8-1286-4d89-bb86-0da50db2b5e2">
</p>

```csharp
private void StaticFlow()
{
    //Define all the node
    Node startNode = Node.Create("Start", "Start", 0, 0, 50, Brushes.LightGray, NodeState.Empty);
    Node step1Node = Node.Create("Step 1", "Step 1", 0, 1, 50, Brushes.LightGray, NodeState.Default);
    Node step2Node = Node.Create("Step 2", "Step 2", 0, 2, 50, Brushes.LightGray, NodeState.Default);
    Node step2BisNode = Node.Create("Step 2b", "Step 2 bis", 1, 2, 50, Brushes.LightGray, NodeState.Default);
    Node step3Node = Node.Create("Step 3", "Step 3", 0, 3, 50, Brushes.LightGray, NodeState.Default);
    Node endNode = Node.Create("End", "End", 0, 4, 50, Brushes.LightGray, NodeState.Empty);

    //Add node connections
    startNode.AddNextNode(step1Node);
    step1Node.AddNextNode(step2Node);
    step1Node.AddNextNode(step2BisNode);
    step2Node.AddNextNode(step3Node);
    step2BisNode.AddNextNode(step3Node);
    step3Node.AddNextNode(endNode);

    //Add nodes to the ObservableCollection
    Nodes.Add(startNode);
    Nodes.Add(step1Node);
    Nodes.Add(step2Node);
    Nodes.Add(step2BisNode);
    Nodes.Add(step3Node);
    Nodes.Add(endNode);

    //Process simulation => Update the node state
    Task.Run(() =>
    {
        // Step 1 Validation
        step1Node.SetState(NodeState.Running);
        System.Threading.Thread.Sleep(1500);
        step1Node.SetState(NodeState.Validate);

        // Step 2 Failure
        step2Node.SetState(NodeState.Running);
        System.Threading.Thread.Sleep(1500);
        step2Node.SetState(NodeState.Failed);

        // Step 2 bis Validation
        step2BisNode.SetState(NodeState.Running);
        System.Threading.Thread.Sleep(1500);
        step2BisNode.SetState(NodeState.Validate);

        // Step 3 Validation
        step3Node.SetState(NodeState.Running);
        System.Threading.Thread.Sleep(1500);
        step3Node.SetState(NodeState.Validate);
    });
}
```

### Dynamic Flow

The dynamic flow demonstrates a process where nodes and connections are created and modified dynamically during execution.

<p align="center">
  <img src="https://github.com/user-attachments/assets/20f8d971-6499-448b-8a9f-55c817c3e75a">
</p>

```csharp
private void DynamicFlow()
{
    //Create the base nodes
    Node startNode = Node.Create("Start", "Start", 0, 0, 50, Brushes.LightGray, NodeState.Empty);
    Node step1Node = Node.Create("Step 1", "Step 1", 0, 1, 50, Brushes.LightGray, NodeState.Default);
    Node endNode = Node.Create("End", "End", 0, 2, 50, Brushes.LightGray, NodeState.Empty);

    //Create the base node connections
    startNode.AddNextNode(step1Node);
    step1Node.AddNextNode(endNode);

    //Add nodes to the ObservableCollection
    Nodes.Add(startNode);
    Nodes.Add(step1Node);
    Nodes.Add(endNode);

    //Process simulation => Dynamicly create and update the node state
    Task.Run(() =>
    {
        // Step 1
        step1Node.SetState(NodeState.Running);
        System.Threading.Thread.Sleep(1500);
        step1Node.SetState(NodeState.Validate);

        endNode.RemovePreviousNodes();
        endNode.MoveTo(0, 3);

        // Step 2 => Creation & connection update
        Node step2Node = Node.Create("Step 2", "Step 2", 0, 2, 50, Brushes.LightGray, NodeState.Default);
        step1Node.AddNextNode(step2Node);
        step2Node.AddNextNode(endNode);
        AddNode(step2Node);
        step2Node.SetState(NodeState.Running);
        System.Threading.Thread.Sleep(1500);
        step2Node.SetState(NodeState.Failed);

        // Step 2 bis => Creation & connection update
        Node step2BisNode = Node.Create("Step 2b", "Step 2 bis", 1, 2, 50, Brushes.LightGray, NodeState.Default);
        step1Node.AddNextNode(step2BisNode);
        step2BisNode.AddNextNode(endNode);
        AddNode(step2BisNode);
        step2BisNode.SetState(NodeState.Running);
        System.Threading.Thread.Sleep(1500);
        step2BisNode.SetState(NodeState.Validate);

        endNode.RemovePreviousNodes();
        endNode.MoveTo(0, 4);

        // Step 3 => Creation & connection update
        Node step3Node = Node.Create("Step 3", "Step 3", 0, 3, 50, Brushes.LightGray, NodeState.Default);
        step2Node.AddNextNode(step3Node);
        step2BisNode.AddNextNode(step3Node);
        step3Node.AddNextNode(endNode);
        AddNode(step3Node);
        step3Node.SetState(NodeState.Running);
        System.Threading.Thread.Sleep(1500);
        step3Node.SetState(NodeState.Validate);
    });
}
```
### Support complex graph and every direction
<p align="center">
  <img src="https://github.com/user-attachments/assets/ebd2d252-e764-40e6-86e3-7f402112fbe0">
</p>

## Contributing

Contributions are welcome!
