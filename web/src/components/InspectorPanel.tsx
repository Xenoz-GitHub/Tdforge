import { useState } from 'react'
import { useScene } from '@/contexts/SceneContext'

export function InspectorPanel() {
  const { objects, selectedId, updateObject } = useScene()
  const target = objects.find(o => o.id === selectedId)
  const [generalOpen, setGeneralOpen] = useState(true)
  const [transformOpen, setTransformOpen] = useState(true)

  if (!target) {
    return (
      <div style={styles.empty}>
        <span style={styles.emptyText}>No object selected</span>
      </div>
    )
  }

  return (
    <div style={styles.container}>
      {/* General Section */}
      <div style={styles.sectionHeader} onClick={() => setGeneralOpen(!generalOpen)}>
        <span style={styles.sectionTitle}>General</span>
        <span style={styles.arrow}>{generalOpen ? '▼' : '▶'}</span>
      </div>
      {generalOpen && (
        <div style={styles.sectionBody}>
          <div style={styles.row}>
            <span style={styles.label}>Name:</span>
            <input
              value={target.name}
              onChange={e => updateObject(target.id, { name: e.target.value })}
              style={styles.input}
            />
          </div>
          <div style={styles.row}>
            <span style={styles.label}>Type:</span>
            <input
              value={target.type}
              onChange={e => updateObject(target.id, { type: e.target.value })}
              style={styles.input}
              placeholder="e.g. player"
            />
          </div>
          <div style={styles.row}>
            <span style={styles.label}>Visible:</span>
            <input
              type="checkbox"
              checked={target.visible}
              onChange={e => updateObject(target.id, { visible: e.target.checked })}
              style={styles.checkbox}
            />
            <span style={{ ...styles.label, marginLeft: 20 }}>Locked:</span>
            <input
              type="checkbox"
              checked={target.locked}
              onChange={e => updateObject(target.id, { locked: e.target.checked })}
              style={styles.checkbox}
            />
          </div>
        </div>
      )}

      {/* Transform Section */}
      <div style={styles.sectionHeader} onClick={() => setTransformOpen(!transformOpen)}>
        <span style={styles.sectionTitle}>Transform</span>
        <span style={styles.arrow}>{transformOpen ? '▼' : '▶'}</span>
      </div>
      {transformOpen && (
        <div style={styles.sectionBody}>
          <div style={styles.row}>
            <span style={styles.label}>X:</span>
            <input
              type="number"
              value={target.x}
              onChange={e => updateObject(target.id, { x: +e.target.value })}
              style={styles.numInput}
            />
            <span style={{ ...styles.label, marginLeft: 8 }}>Y:</span>
            <input
              type="number"
              value={target.y}
              onChange={e => updateObject(target.id, { y: +e.target.value })}
              style={styles.numInput}
            />
          </div>
          <div style={styles.row}>
            <span style={styles.label}>W:</span>
            <input
              type="number"
              value={target.width}
              onChange={e => updateObject(target.id, { width: +e.target.value })}
              style={styles.numInput}
              min={1}
            />
            <span style={{ ...styles.label, marginLeft: 8 }}>H:</span>
            <input
              type="number"
              value={target.height}
              onChange={e => updateObject(target.id, { height: +e.target.value })}
              style={styles.numInput}
              min={1}
            />
          </div>
          <div style={styles.row}>
            <span style={styles.label}>Rot:</span>
            <input
              type="number"
              value={target.rotation}
              onChange={e => updateObject(target.id, { rotation: +e.target.value })}
              style={styles.numInput}
            />
            <span style={{ ...styles.label, marginLeft: 8 }}>Scl X:</span>
            <input
              type="number"
              value={target.scaleX}
              onChange={e => updateObject(target.id, { scaleX: +e.target.value })}
              style={styles.numInput}
              min={0.01}
              step={0.1}
            />
          </div>
        </div>
      )}

      {/* Appearance Section */}
      <div style={styles.sectionHeader}>
        <span style={styles.sectionTitle}>Appearance</span>
      </div>
      <div style={styles.sectionBody}>
        <div style={styles.row}>
          <span style={styles.label}>Sprite:</span>
          <input
            value={target.sprite}
            onChange={e => updateObject(target.id, { sprite: e.target.value })}
            style={styles.input}
            placeholder="Select sprite..."
          />
        </div>
        <div style={styles.row}>
          <span style={styles.label}>Color:</span>
          <div style={{
            ...styles.colorSwatch,
            background: target.color,
          }} />
          <span style={{ ...styles.label, marginLeft: 8 }}>Alpha:</span>
          <input
            type="number"
            value={target.opacity}
            onChange={e => updateObject(target.id, { opacity: +e.target.value })}
            style={styles.numInput}
            min={0}
            max={255}
          />
        </div>
      </div>
    </div>
  )
}

const styles: Record<string, React.CSSProperties> = {
  container: {
    overflow: 'auto',
    flex: 1,
  },
  empty: {
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    height: '100%',
  },
  emptyText: {
    color: '#443055',
    fontSize: 13,
  },
  sectionHeader: {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
    padding: '5px 8px',
    background: '#2a1838',
    cursor: 'pointer',
    borderBottom: '1px solid #332044',
  },
  sectionTitle: {
    color: '#9880a8',
    fontSize: 10,
    fontWeight: 700,
  },
  arrow: {
    color: '#9880a8',
    fontSize: 8,
  },
  sectionBody: {
    padding: '4px 8px',
    borderBottom: '1px solid #221430',
  },
  row: {
    display: 'flex',
    alignItems: 'center',
    gap: 4,
    marginBottom: 3,
  },
  label: {
    color: '#c0b0d0',
    fontSize: 12,
    minWidth: 40,
  },
  input: {
    flex: 1,
    background: '#1a1020',
    color: '#e0d0f0',
    border: '1px solid #443055',
    padding: '2px 6px',
    fontSize: 12,
    borderRadius: 2,
    fontFamily: 'inherit',
  },
  numInput: {
    width: 60,
    background: '#1a1020',
    color: '#e0d0f0',
    border: '1px solid #443055',
    padding: '2px 4px',
    fontSize: 12,
    borderRadius: 2,
    fontFamily: 'inherit',
  },
  checkbox: {
    accentColor: '#8b4fc8',
  },
  colorSwatch: {
    width: 22,
    height: 22,
    borderRadius: 3,
    border: '1px solid #443055',
  },
}
