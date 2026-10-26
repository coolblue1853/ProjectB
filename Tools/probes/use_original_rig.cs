// 비교 촬영용(플레이 중, 저장 안 함): 픽셀 리그를 숨기고 예전 AnyPortrait 주인공으로 되돌린다 — 기본 이동·점프 기준 영상(Q-423)
var pc = PlayerController.instance;
var pix = pc.mainCharacter.transform.parent.Find("PixelRig_Adventurer");
if (pix != null) pix.gameObject.SetActive(false);
pc.mainCharacter.gameObject.SetActive(true);
pc.UseRig(PortraitRig.For(pc.mainCharacter));
pc.Rig.Play("Idle");
return "original rig on: " + pc.Rig.GetType().Name;
