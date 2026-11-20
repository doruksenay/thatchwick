import os
import sys
import unittest

sys.path.insert(0, os.path.join(os.path.dirname(__file__), ".."))
import sabit_metin  # noqa: E402


def diff(path, removed=(), added=(), start=10):
    lines = [f"diff --git a/{path} b/{path}", f"--- a/{path}", f"+++ b/{path}", f"@@ -{start},{len(removed)} +{start},{len(added)} @@"]
    lines += ["-" + r for r in removed]
    lines += ["+" + a for a in added]
    return "\n".join(lines) + "\n"


class SabitMetinTest(unittest.TestCase):
    def test_satir_olcusu(self):
        self.assertEqual(sabit_metin.literals('    ShowMessage("Kaydedildi, köy uyuyor");'), ['"Kaydedildi, köy uyuyor"'])
        self.assertEqual(sabit_metin.literals('    ShowMessage(T("ui.save.done"));'), [])
        self.assertEqual(sabit_metin.literals('    // "Köylü" eski addı'), [])
        self.assertEqual(sabit_metin.literals('    Debug.LogWarning("İçerik hatası");'), [])
        self.assertEqual(sabit_metin.literals('    int x = 3; // "şaka"'), [])
        self.assertEqual(sabit_metin.literals('    var id = "item.egg";'), [])
        self.assertEqual(sabit_metin.literals('    names = { "Sarıkız", "Poyraz" };   // loc-yok: at adları çevrilmez'), [])

    def test_yeni_metin_yakalanir_satir_numarasiyla(self):
        d = diff("Assets/Scripts/Game/X.cs", added=['    int a = 1;', '    ShowMessage("Sırık dikildi");'], start=40)
        self.assertEqual(sabit_metin.scan(d), [("Assets/Scripts/Game/X.cs", 41, '"Sırık dikildi"')])

    def test_tasinan_metin_sayilmaz(self):
        d = diff("Assets/Scripts/Game/Eski.cs", removed=['    Label("Köy işi");']) + \
            diff("Assets/Scripts/Game/Yeni.cs", added=['        Label("Köy işi");', '        Label("Yeni düğme");'])
        self.assertEqual([lit for _, _, lit in sabit_metin.scan(d)], ['"Yeni düğme"'])

    def test_anahtara_tasimak_temiz(self):
        d = diff("Assets/Scripts/Core/Y.cs", removed=['    return "Sırığı zaten dikili.";'],
                 added=['    return CoreText.T("ui.farm.block.already_staked");'])
        self.assertEqual(sabit_metin.scan(d), [])

    def test_cs_disindaki_dosya_sayilmaz(self):
        d = diff("Assets/Scripts/Game/notlar.md", added=['"Türkçe not"'])
        self.assertEqual(sabit_metin.scan(d), [])


if __name__ == "__main__":
    unittest.main()
