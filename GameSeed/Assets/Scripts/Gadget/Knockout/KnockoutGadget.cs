// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// [CreateAssetMenu(menuName = "Gadgets/KnockoutGadget")]

// public class KnockoutGadget : BaseGadget
// {
//     public override void Apply(StickData target)
//     {
//         target.canActivateSafeArea++;
//     }

//     public override void Remove(StickData target)
//     {
//         // Mengurangi kuota saat gadget dilepas
//         if (target.canActivateSafeArea > 0) 
//             target.canActivateSafeArea--;
//         else 
//             Debug.LogWarning("Tidak ada kuota Safe Area yang bisa dikurangi.");
//     }
// }