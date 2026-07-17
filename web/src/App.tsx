import { useState } from 'react'
import { Allotment } from 'allotment'
import 'allotment/dist/style.css'
import { EditorProvider } from '@/contexts/EditorContext'
import { SceneProvider } from '@/contexts/SceneContext'
import { ConsoleProvider } from '@/contexts/ConsoleContext'
import { GameProvider } from '@/contexts/GameContext'
import { MenuBar } from '@/components/MenuBar'
import { WorkspaceTabs } from '@/components/WorkspaceTabs'
import { Toolbar2D } from '@/components/Toolbar2D'
import { StatusBar } from '@/components/StatusBar'
import { ConsolePanel } from '@/components/ConsolePanel'
import { SplitLayout } from '@/components/SplitLayout'
import './App.css'

function App() {
  const [activeTab, setActiveTab] = useState('2d')
  const [tool, setTool] = useState('select')
  const [snapEnabled, setSnapEnabled] = useState(true)
  const [gridEnabled, setGridEnabled] = useState(true)

  return (
    <EditorProvider>
      <SceneProvider>
        <ConsoleProvider>
          <GameProvider>
            <div style={styles.root}>
              <MenuBar />
              <WorkspaceTabs activeTab={activeTab} onTabChange={setActiveTab} />
              <Toolbar2D
                tool={tool}
                onToolChange={setTool}
                snapEnabled={snapEnabled}
                onSnapToggle={() => setSnapEnabled(!snapEnabled)}
                gridEnabled={gridEnabled}
                onGridToggle={() => setGridEnabled(!gridEnabled)}
              />
              <div style={styles.main}>
                <Allotment vertical defaultSizes={[400, 150]}>
                  <SplitLayout />
                  <div style={styles.consoleContainer}>
                    <ConsolePanel />
                  </div>
                </Allotment>
              </div>
              <StatusBar
                tool={tool.charAt(0).toUpperCase() + tool.slice(1)}
                mousePos=""
                zoom="100%"
              />
            </div>
          </GameProvider>
        </ConsoleProvider>
      </SceneProvider>
    </EditorProvider>
  )
}

const styles: Record<string, React.CSSProperties> = {
  root: {
    width: '100vw',
    height: '100vh',
    display: 'flex',
    flexDirection: 'column',
    overflow: 'hidden',
    background: '#1a1020',
    color: '#e0d0f0',
    fontFamily: "'Segoe UI', 'Roboto', sans-serif",
  },
  main: {
    flex: 1,
    overflow: 'hidden',
  },
  consoleContainer: {
    height: '100%',
    display: 'flex',
    flexDirection: 'column',
  },
}

export default App
