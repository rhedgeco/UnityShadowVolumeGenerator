*This repo is under heavy development.*
![](Resources/shadows.gif)
# Unity Shadow Volume Generator (Stencil Shadows)
Stencil shadow volumes are incredibly useful for certain styles of shadows in games.
Then why are they so hard to find info on and make?

This repository hopefully addresses some of that problem.

It's also blazing fast. It integrates using the job system and burst compiler.
This enables the volume extrusions to happen in fractions of a millisecond.

## Limitations
- Limited to URP 11 and up with forward renderer. URP 11 reintroduced screen space shadows.
- Only renders shadows from main sun source

## What are Shadow Volumes?
Shadow volumes are recognized by the CRISP and ACCURATE shadows that they cast.

When you think of super accurate shadows, you would be mistaken to think that this is cutting edge tech.
Shadow volumes are actually quite old fashioned.
Here is an example from Doom 3.
![Doom 3](Resources/Doom3.jpg)

You see those perfectly crisp polygonal shadows? 
I know, not very realistic, but it was the style of an era.
Those were created using shadow volumes.

We essentially find the outline of an object from the lights perspective,
and then create a mesh that casts into the scene.
Wherever that mesh intersects... well... its shadowed.

We see this still used today in more cartoon looking games as well.
Here is an example from boomerang fu. <br>
*(to be fair, i dont know if they use this. but it damn well looks like it.)*
![Boomerang Fu](Resources/BoomerangFu.jpg)

## [MIT License](LICENSE.md)