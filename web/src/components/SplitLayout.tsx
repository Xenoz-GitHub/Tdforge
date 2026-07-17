import { Allotment } from 'allotment'
import 'allotment/dist/style.css'
import { SceneTree } from '@/components/SceneTree'
import { FileTree } from '@/components/FileTree'
import { Outline } from '@/components/Outline'
import { InspectorPanel } from '@/components/InspectorPanel'
import { GamePreview } from '@/components/GamePreview'
import { useScene } from '@/contexts/SceneContext'
import { useGame } from '@/contexts/GameContext'

export function SplitLayout() {
  const { objects, selectedId, selectObject } = useScene()
  const { isRunning } = useGame()

  const files = [
    { name: 'main.td', path: 'main.td' },
    { name: 'sprite.td', path: 'sprite.td' },
    { name: 'player.td', path: 'player.td' },
  ]

  const symbols = [
    { name: 'start', type: 'function' as const, line: 3 },
    { name: 'tick', type: 'function' as const, line: 7 },
    { name: 'Player', type: 'class' as const, line: 14 },
    { name: 'score', type: 'variable' as const, line: 1 },
  ]

  return (
    <Allotment>
      {/* LEFT PANEL */}
      <Allotment vertical defaultSizes={[200, 160, 100]} minSize={80}>
        <div style={styles.panel}>
          <div style={styles.panelHeader}>SCENE</div>
          <SceneTree objects={objects} selectedId={selectedId} onSelect={selectObject} />
        </div>
        <div style={styles.panel}>
          <div style={styles.panelHeader}>PROJECT</div>
          <FileTree files={files} onOpenFile={() => {}} activeFile="main.td" />
        </div>
        <div style={styles.panel}>
          <div style={styles.panelHeader}>OUTLINE</div>
          <Outline symbols={symbols} onNavigate={() => {}} />
        </div>
      </Allotment>

      {/* CENTER PANEL */}
      <div style={{ ...styles.panel, ...styles.centerPanel }}>
        {isRunning ? (
          <GamePreview width={640} height={480} />
        ) : (
          <div style={styles.editorPlaceholder}>
            <span style={{ color: '#443055', fontSize: 13 }}>
              Code editor placeholder — Monaco will mount here
            </span>
          </div>
        )}
      </div>

      {/* RIGHT PANEL */}
      <div style={styles.panel}>
        <div style={styles.panelHeader}>INSPECTOR</div>
        <InspectorPanel />
      </div>
    </Allotment>
  )
}

const styles: Record<string, React.CSSProperties> = {
  panel: {
    background: '#221430',
    display: 'flex',
    flexDirection: 'column',
    overflow: 'hidden',
  },
  panelHeader: {
    padding: '6px 8px',
    color: '#9880a8',
    fontSize: 11,
    fontWeight: 700,
    background: '#2a1838',
    borderBottom: '1px solid #332044',
    flexShrink: 0,
  },
  centerPanel: {
    background: '#1a1020',
    flex: 1,
  },
  editorPlaceholder: {
    flex: 1,
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    minHeight: 200,
  },
}
