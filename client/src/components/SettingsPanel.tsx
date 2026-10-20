import { useState, type CSSProperties, type ReactNode } from 'react'
import { ActionIcon, Affix, Checkbox, ColorSwatch, Group, Paper, Radio, Select, SimpleGrid, Slider, Stack, Text, Title } from '@mantine/core'
import { themes } from '../themes'
import { useSettings } from '../context/SettingsContext'
import './SettingsPanel.css'

function Section({ title, children }: { title: string; children: ReactNode }) {
  return (
    <Stack gap={8}>
      <Text size="xs" fw={600} c="dimmed" tt="uppercase" lts={0.8}>{title}</Text>
      {children}
    </Stack>
  )
}

interface LayerToggleProps {
  label: string
  icon: ReactNode
  checked: boolean
  onChange: (checked: boolean) => void
}

function LayerToggle({ label, icon, checked, onChange }: LayerToggleProps) {
  return (
    <Checkbox size="xs" checked={checked} onChange={e => onChange(e.currentTarget.checked)} label={<Group component="span" gap={8} wrap="nowrap">{icon}{label}</Group>} />
  )
}

function Dot({ color, size = 10 }: { color: string; size?: number }) {
  return <ColorSwatch component="span" color={color} size={size} withShadow={false} />
}

interface IntervalSelectProps {
  label: string
  options: number[]
  value: number
  onChange: (ms: number) => void
}

function IntervalSelect({ label, options, value, onChange }: IntervalSelectProps) {
  return (
    <Group justify="space-between" wrap="nowrap">
      <Text size="sm">{label}</Text>
      <Select size="xs" w={80} aria-label={`${label} refresh interval`} allowDeselect={false} data={options.map(ms => ({ value: ms, label: `${ms / 1000} s` }))} value={value} onChange={ms => ms !== null && onChange(ms)} />
    </Group>
  )
}

export default function SettingsPanel() {
  const [open, setOpen] = useState(false)
  const { settings, update } = useSettings()
  const t = themes[settings.themeId]

  return (
    <Affix position={{ top: 10, left: 10 }} zIndex={10} withinPortal={false}>
      <ActionIcon variant="default" size={36} fz={18} style={{ boxShadow: 'var(--mantine-shadow-sm)' }} onClick={() => setOpen(o => !o)} title="Settings" aria-label="Toggle settings">⚙</ActionIcon>

      {open && (
        <Paper withBorder shadow="md" p="md" mt={8} w={220}>
          <Title order={3} fz={14} fw={600} tt="uppercase" lts={0.5} mb={14}>Settings</Title>

          <Stack gap={18}>
            <Section title="Theme">
              <Radio.Group value={settings.themeId} onChange={themeId => update({ themeId })}>
                <SimpleGrid cols={2} spacing={6}>
                  {Object.values(themes).map(theme => {
                    const active = settings.themeId === theme.id
                    return (
                      <Radio.Card key={theme.id} value={theme.id} p={8} bg="dark.8" style={{ borderColor: active ? theme.roadMotorway : undefined }}>
                        <Stack gap={5} align="center">
                          <span className="theme-preview" style={{ '--theme-road': theme.roadMotorway, '--theme-bg': theme.background } as CSSProperties} />
                          <Text size="xs" c={active ? 'bright' : 'dimmed'}>{theme.label}</Text>
                        </Stack>
                      </Radio.Card>
                    )
                  })}
                </SimpleGrid>
              </Radio.Group>
            </Section>

            <Section title="Live data">
              <LayerToggle label="Buses" icon={<Dot color={t.bus} />} checked={settings.showBuses} onChange={showBuses => update({ showBuses })} />
              <LayerToggle label="Trains" icon={<Dot color={t.train} />} checked={settings.showTrains} onChange={showTrains => update({ showTrains })} />
              <LayerToggle label="Incidents" icon={<Dot color={t.incident} />} checked={settings.showIncidents} onChange={showIncidents => update({ showIncidents })} />
              <Group justify="space-between" wrap="nowrap">
                <Text size="sm">Icon size</Text>
                <Group gap={6} wrap="nowrap">
                  <Slider w={80} size="sm" min={0.2} max={1.0} step={0.05} label={null} value={settings.vehicleIconSize} onChange={vehicleIconSize => update({ vehicleIconSize })} />
                  <Text size="sm" miw={28} ta="right">{settings.vehicleIconSize.toFixed(2)}</Text>
                </Group>
              </Group>
            </Section>

            <Section title="Map overlays">
              <LayerToggle label="Bus routes" icon={<span className="line-icon" style={{ background: t.busRoute }} />} checked={settings.showBusRoutes} onChange={showBusRoutes => update({ showBusRoutes })} />
              <LayerToggle label="Train routes" icon={<span className="line-icon" style={{ background: t.trainRoute }} />} checked={settings.showTrainRoutes} onChange={showTrainRoutes => update({ showTrainRoutes })} />
              <LayerToggle label="Metro routes" icon={<span className="line-icon" style={{ background: t.metroRoute }} />} checked={settings.showMetroRoutes} onChange={showMetroRoutes => update({ showMetroRoutes })} />
              <LayerToggle label="Ferry routes" icon={<span className="line-icon" style={{ background: t.ferryRoute }} />} checked={settings.showFerryRoutes} onChange={showFerryRoutes => update({ showFerryRoutes })} />
              <LayerToggle label="Light rail routes" icon={<span className="line-icon" style={{ background: t.lightRailRoute }} />} checked={settings.showLightRailRoutes} onChange={showLightRailRoutes => update({ showLightRailRoutes })} />
              <LayerToggle label="Bus stops" icon={<Dot color={t.busStop} />} checked={settings.showBusStops} onChange={showBusStops => update({ showBusStops })} />
              <LayerToggle label="Rail lines" icon={<span className="line-icon dashed" style={{ '--line-color': t.rail } as CSSProperties} />} checked={settings.showRail} onChange={showRail => update({ showRail })} />
              <LayerToggle label="Motorways" icon={<span className="line-icon" style={{ background: t.roadMotorway }} />} checked={settings.showMotorways} onChange={showMotorways => update({ showMotorways })} />
              <LayerToggle label="Train stations" icon={<Dot color={t.trainStation} />} checked={settings.showTrainStations} onChange={showTrainStations => update({ showTrainStations })} />
              <LayerToggle label="Traffic signals" icon={<Dot color={t.trafficSignal} size={7} />} checked={settings.showTrafficSignals} onChange={showTrafficSignals => update({ showTrafficSignals })} />
            </Section>

            <Section title="Refresh intervals">
              <IntervalSelect label="Vehicles" options={[2000, 5000, 10000, 30000]} value={settings.vehiclesPollMs} onChange={vehiclesPollMs => update({ vehiclesPollMs })} />
              <IntervalSelect label="Incidents" options={[5000, 10000, 30000, 60000]} value={settings.incidentsPollMs} onChange={incidentsPollMs => update({ incidentsPollMs })} />
            </Section>
          </Stack>
        </Paper>
      )}
    </Affix>
  )
}
