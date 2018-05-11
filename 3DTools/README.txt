This README gives a brief overview of the major classes used to enable 
interactive 2D on 3D, a high level overview of how it works, 
as well as a list of known issues and changes that can be made to get 
around them.

---------------
Class Overview:
---------------
There are three main classes that are used to enable interacting with 2D 
content on 3D objects: Viewport3DDecorator, Interactive3DDecorator and 
InteractiveVisual3D.  As a client of the 2D on 3D code, only Interactive3DDecorator 
and InteractiveVisual3D need to be used.  These three classes are explained next.

Viewport3DDecorator:
--------------------
The Viewport3DDecorator is used to extend the functionality of a Viewport3D.  
It does so by allowing UIElements to be placed in front of and behind the 
Viewport3D that is being decorated.  This enables, for instance, the 
Interactive3DDecorator to place its hidden layer on top of the Viewport3D it 
is decorating, or the trackball to put a pane of glass beneath the Viewport3D 
it is decorating, so it can be notified when the mouse is clicked within 
the bounds of the Viewport3D.  The UIElements that occur in front of and behind the 
Viewport3D are exposed via the PreViewportChildren and PostViewportChildren dependency 
properties.  The Viewport3D or Viewport3DDecorator that is decorated is exposed 
via the Content property of the class.  The class takes care of laying out all 
of its children, but this can be overridden by derived classes if desired.    

Interactive3DDecorator:
-----------------------
The Interactive3DDecorator is a subclass of the Viewport3DDecorator, and uses 
its PostViewportChildren property to position the hidden visual layer which 
actually provides the interaction.  This class provides interactivity to a Viewport3D.  

In XAML, the Interactive3DDecorator is used as follows:

<local:Interactive3DDecorator>
	<Viewport3D>
		?
	</Viewport3D>
</local:Interactive3DDecorator>

InteractiveVisual3D:
--------------------
The final class, the InteractiveVisual3D, is used to create the interactive 3D content 
and also to signal to the Interactive3DDecorator that this Visual3D is intended to 
be interacted with.  InteractiveVisual3D is a subclass of ModelVisual3D and provides 
the following dependency properties:

?Geometry      - The Geometry3D that is to become the content of the InteractiveVisual3D.
?Visual        ?The Visual that is to be used in a VisualBrush, which will then 
					be used in a material for the geometry for the InteractiveVisual3D.
?Material      ?A user specified material, with the IsInteractiveMaterial attached 
					property being used to mark the locations where the user wants the 
					VisualBrush used to represent the Visual, to be placed.  By default, 
					a DiffuseMaterial is used.
?IsBackVisible ?Indicates whether the material used for the front face should also be 
					mirrored on the back face.

The first two properties, Geometry and Visual, are the primary ones needed.  They allow 
the geometry for the Visual3D to be set, as well as the Visual that should appear on that 
geometry.  Note: the texture coordinates used for the Geometry must be in the range 
(0,0) to (1,1) to enable interaction with the Visual.  If they are outside this range, the
user will be interacting outside of the bounds of the Visual. 

The others allow for fine tuning of the object抯 appearance.  The Material property allows 
a user to create their own material for the object.  Because there potentially need to be 
things added above the Visual specified by the user, and because parameters of the 
VisualBrush are modified, the user does not directly specify the VisualBrush used.  Instead, 
they specify using the IsInteractiveMaterial attached property, which material they wish to 
make 搃nteractive?(i.e. set the VisualBrush created using the passed in Visual as the 
Brush for that material).  As an example, the following code sets the material to be composed 
of a DiffuseMaterial, which will contain the visual brush, and a SpecularMaterial.

<local:InteractiveVisual3D.Material>
	<MaterialGroup>
		<DiffuseMaterial local:InteractiveVisual3D.IsInteractiveMaterial="True"/>
		<SpecularMaterial Brush="Red" />
	</MaterialGroup>
</local:InteractiveVisual3D.Material>
          
Finally, IsBackVisible sets the material used on the front face to also be used on the back face.  
Currently, the Interactive3DDecorator does not distinguish between front and back faces, so this 
allows for there to be interaction with the back face as well.

The following is an example use of the InteractiveVisual3D in XAML:

<local:InteractiveVisual3D Geometry="{StaticResource PlaneMesh}">
    <local:InteractiveVisual3D.Visual>
        <StackPanel>
            <Label Content="Sample UI 2" />
            <Button Content="Close Window"/>
            <TextBox />
        </StackPanel>
    </local:InteractiveVisual3D.Visual>
</local:InteractiveVisual3D>

-----------------
How does it work:
-----------------
At a very high level, the interaction with 2D on 3D is achieved by really interacting 
with a hidden version of that 2D content in 2D.  The 2D is positioned such that the 
point in 3D the mouse is over is the exact same point as the mouse is over on the 
hidden 2D version.  Then, when the user clicks, etc?they are interacting with exactly 
the same location.  If you want to see this for yourself, you can set the 揇ebug?
property on Interactive3DDecorator to true, which makes the hidden layer partially visible.

The Interactive3DDecorator then consists of two elements: the 3D content that is displayed 
within it and a hidden layer that is used to position and display the 2D content that is 
being interacted with.  Depending on what 2D content on 3D is being interacted with, the 
hidden layer changes to hold that 2D content.  For example, consider a simple case of a
button, with text "Button", placed on a sphere.  When the mouse moves over the 揃?in 
the button on the sphere, the hidden layer (the button) is moved such that the mouse is 
over the same point in the hidden layer as it is in the 3D scene.   
  
To figure out where to position the hidden layer works as follows.  When the mouse moves 
in the 3D scene, a ray is shot in to the 3D scene to see if it intersects any object.  
If it hit an object, and it is an InteractiveVisual3D, we can use the return parameters 
from the intersection to compute the texture coordinate that was hit.  Then from these, 
we can map from the (u,v) value of the texture coordinate, on to an x,y point on the 2D 
visual, which is the point we need to place under the mouse.  More specifically, the code 
assumes texture coordinates are all in the range (0,0) to (1,1) ?i.e. upper left to lower 
right of the image (this is important to know, since your texture coordinates need to 
be within this range to enable interaction with the 2D content).  Then the point on the 
2D object that was hit is simply (u * Width, v * Height). 

There抯 one very interesting 揼otcha?though: what happens when one of the 2D objects grabs 
capture and then you move off the 3D mesh it is on?  For example, you select some text, and 
then move the mouse above that selected text, or click and hold on a button, and then move 
away from it.  Correct hidden content positioning becomes more complicated in this case.  
The problem becomes difficult for many reasons.  In the normal 2D situation, both the mouse 
position and the 2D content exist in the same plane.  The transformation that is applied to 
the 2D content can be used to transform the mouse position to the content抯 local coordinate 
system.  However in 3D, due to the projection of 3D on to a 2D plane, the mouse抯 position 
actually corresponds to a line in 3D space.  In addition, the element with capture could also 
be mapped to any arbitrary geometry.  When the mouse is over the 3D object, hit testing tells 
us where it is relative to the 2D visual.  When it is off the 3D object, due to the above 
issues, there is no longer a straight forward answer to this question: the 2D point corresponds 
to a 3D line and the 2D content could be on arbitrary 3D geometry.  Also, because the element 
has capture, it wants to receive all events.  Before, we only needed to be sure that the mouse 
was over the correct object at all times.  Now we need to position the hidden visual such that 
it is in the proper position relative to the object that has capture.

To solve this issue, the code takes the following approach:  The overall idea is to reduce 
the 3D problem back to 2D.  In the normal 2D case, the transformations applied to the 
content can be used to convert the mouse position to the content抯 local coordinate 
system.  This transformed position then lets the content know where the mouse is relative 
to it.  In 3D, due to the many orientations of the geometry and texture coordinate layouts, 
it抯 difficult to say where a 3D point is in the relative coordinate system of the 2D 
content on 3D.  To approximate this, the outline of the 2D content on 3D, after it has 
been projected to screen space, is computed, and then the mouse is positioned based on 
this projection. 
    
After the outline is available, The closest point on this outline to the mouse position 
is computed, and then this point on the outline is considered what was 揾it?and it is 
placed under the moue position.  Since we place the mouse by the closest edge point, 
the interaction tends to behave as it would in 2D, since we position the hidden content 
based on what the mouse is closest to on the 2D content on 3D.  By placing it at the 
closest edge point, we are explicitly stating about where we expect the mouse to be 
relative to the 2D on 3D element抯 orientation.

This method helps provide an intuitive response to the interaction, since the interaction 
happens with the closest point on the object with capture to the mouse.

-------------
Known Issues:
-------------
?Inking support:  
Inking is possible with the 2D on 3D code, but it requires you to set the ContainsInk 
dependency property on Interactive3DDecorator to true.  This then causes an InvalidateArrange
to occur whenever the mouse is moved. Because the InvalidateArrange could potentially be 
computationally costly, this property is set to false by default.  See the Channel9Demo included 
in the Samples directory, for an example of how to set this dependency property.

?Styles and Triggers
Because the Interactive3DDecorator is changing the hidden content based on what 2D on 3D 
object the mouse is over, certain styling issues can appear.  This is due to the fact that 
the 2D Visuals that appears on the 3D objects are being added and removed from the Visual 
tree depending on what 3D object the mouse is over.  To get around this problem, the code 
would need to be modified to keep every 2D Visual of an Interactive3DVisual in the hidden 
layer at all times, and then simply move around whichever one is currently being interacted with.  
This will then always have the 2D on 3D content directly in the visual tree at all times 
(rather than appearing within a visual brush).
